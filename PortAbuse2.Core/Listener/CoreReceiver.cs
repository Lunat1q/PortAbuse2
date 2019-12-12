using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using PacketDotNet;
using PortAbuse2.Core.ApplicationExtensions;
using PortAbuse2.Core.Common;
using PortAbuse2.Core.Geo;
using PortAbuse2.Core.Parser;
using PortAbuse2.Core.Proto;
using PortAbuse2.Core.Result;
using PortAbuse2.Core.WindowsFirewall;
using SharpPcap;
using SharpPcap.WinPcap;

namespace PortAbuse2.Core.Listener
{
    public class CoreReceiver
    {
        private bool _hideOld;

        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public ObservableCollection<ResultObject> ResultObjects = new ObservableCollection<ResultObject>();

        private int _collectionAccessWrite;
        public bool ContinueCapturing { get; private set; } //A flag to check if packets are to be captured or not
        public AppEntry SelectedAppEntry;
        private string _interfaceLocalIp;
        private IEnumerable<IApplicationExtension> _currentExtensions;
        private ICaptureDevice _captureDevice;

        //private readonly string _logFolder = "raw";
        private const int OldTimeLimitSeconds = 30;

        private const int RegularActionsDelay = 500;

        private bool _minimizeHostname;

        public delegate void MessageDetectedEventHandler(
            IPAddress ipDest,
            IPAddress ipSource,
            byte[] data,
            bool direction,
            ResultObject resultObject,
            IEnumerable<Tuple<Protocol, ushort>> protocol
        );

        public void Stop()
        {
            this.ContinueCapturing = false;
            this._captureDevice?.StopCapture();
            if (this._currentExtensions != null)
            {
                foreach (var ext in this._currentExtensions)
                {
                    this.Received -= ext.PackageReceived;
                    ext.Stop();
                }
            }
        }

        public event MessageDetectedEventHandler Received;

        protected Func<ResultObject, bool> InvokedAdd { get; set; }

        public bool BlockNew = false;
        public bool HideSmallPackets;
        private bool _forceShowHiddenIps;

        protected CoreReceiver(bool minimizeHostname = false, bool hideOld = false, bool hideSmall = false)
        {
            this._minimizeHostname = minimizeHostname;
            this._hideOld = hideOld;
            this.HideSmallPackets = hideSmall;
        }

        public void HideOld()
        {
            this._hideOld = true;
        }

        public void ShowOld()
        {
            this._hideOld = false;
            Task.Delay(RegularActionsDelay);
            foreach (var ro in this.TryGetAccessToCollection())
            {
                ro.Old = false;
            }
        }

        public void MinimizeHostnames()
        {
            this._minimizeHostname = true;
            Task.Delay(RegularActionsDelay);
            foreach (var ro in this.TryGetAccessToCollection())
            {
                ro.Hostname = DnsHost.MinimizeHostname(ro.DetectedHostname);
            }
        }

        public void SetForceShowHiddenIps(bool forceShow = true)
        {
            this._forceShowHiddenIps = forceShow;
            Task.Delay(RegularActionsDelay);
            foreach (var ro in this.TryGetAccessToCollection())
            {
                ro.ForceShow = this._forceShowHiddenIps;
            }
        }

        public void UnminimizeHostnames()
        {
            this._minimizeHostname = false;
            Task.Delay(RegularActionsDelay);
            foreach (var ro in this.TryGetAccessToCollection())
            {
                ro.Hostname = ro.DetectedHostname;
            }
        }

        private async Task CleanupDupes() //TODO: Test if its needed at all
        {
            while (this.ContinueCapturing)
            {
                this.StartWrite();
                var dupes = this.ResultObjects.GroupBy(x => x.ShowIp).Where(x => x.Count() > 1);
                foreach (var dupe in dupes)
                {
                    if (dupe.Count() <= 1) continue;

                    var main = dupe.FirstOrDefault();

                    if (main == null) continue;

                    foreach (var second in dupe.Where(x => x != main))
                    {
                        main.PackagesReceived += second.PackagesReceived;
                        this.ResultObjects.Remove(second);
                    }
                }

                this.EndWrite();
                await Task.Delay(5000);
            }
        }

        private async Task HideOldTask()
        {
            while (this.ContinueCapturing)
            {
                if (this._hideOld)
                {
                    var nowMinusShift = DateTime.UtcNow.ToUnixTime() - OldTimeLimitSeconds * 1000;
                    foreach (var ro in this.TryGetAccessToCollection())
                    {
                        if (ro != null)
                        {
                            if (ro.LastReceivedTime < nowMinusShift && !ro.Old)
                            {
                                ro.Old = true;
                            }
                            else if (ro.Old && ro.LastReceivedTime > nowMinusShift)
                            {
                                ro.Old = false;
                            }
                        }
                    }
                }
                await Task.Delay(1000);
            }
        }

        public void Clear()
        {
            this.StartWrite();
            this.ResultObjects.Clear();
            this.EndWrite();
        }

        private void EndWrite()
        {
            this._collectionAccessWrite--;
        }

        private void StartWrite()
        {
            this._collectionAccessWrite++;
        }

        private void InitExtensions()
        {
            this._currentExtensions = ExtensionsRepository.GetExtensionsForApp(this.SelectedAppEntry.Name);
            foreach (var ext in this._currentExtensions)
            {
                this.Received += ext.PackageReceived;
                ext.ResultObjectRef = this.ResultObjects;
                ext.Start();
            }
        }

        public void StartListener(string ipInterface)
        {
            this.ContinueCapturing = true;
            this.StartWrite();
            this.ResultObjects.Clear();
            this.EndWrite();
            Task.Run(this.HideOldTask);
            Task.Run(this.CleanupDupes);
            this.InitExtensions();


            this._interfaceLocalIp = ipInterface;
            // метод для получения списка устройств
            CaptureDeviceList deviceList = CaptureDeviceList.Instance;
            foreach (var device in deviceList)
            {
                if (!(device is WinPcapDevice pCapDevice)) continue;
                var winDevice = pCapDevice;
                if (winDevice.Addresses.Any(x => x.Addr.ToString() == this._interfaceLocalIp))
                {
                    this._captureDevice = winDevice;
                    break;
                }
            }
            if (this._captureDevice == null) //просрали девайс, берем первый и молимся
                this._captureDevice = deviceList[0];
            // регистрируем событие, которое срабатывает, когда пришел новый пакет
            this._captureDevice.OnPacketArrival += this.Receiver_OnPacketArrival;
            // открываем в режиме promiscuous, поддерживается также нормальный режим
            this._captureDevice.Open(DeviceMode.Promiscuous, 1000);
            // начинаем захват пакетов
            this._captureDevice.StartCapture();
        }

        private void Receiver_OnPacketArrival(object sender, CaptureEventArgs e)
        {
            // парсинг всего пакета
            try
            {
                var packet = Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);
                Task.Run(() => this.ParsePacket(packet));
            }
            catch
            {
                // ignored
            }
        }

        private static byte[] GetData(TcpPacket tcp, UdpPacket udp)
        {
            return tcp?.PayloadData ?? udp.PayloadData;
        }

        private async Task ParsePacket(Packet packet)
        {
            var port = PackageHelper.GetPorts(packet, out IpPacket ipPacket, out TcpPacket tcpPacket,
                out UdpPacket udpPacket);

            var portsMatch = this.SelectedAppEntry.AppPort.Any(
                    x => port != null && port.Any(v => v.Item2 == x.UPortNumber && x.Protocol == v.Item1));
            if (!portsMatch) return;

            var fromMe = ipPacket.SourceAddress.ToString() == this._interfaceLocalIp;
            var existedDetection = this.GetExistedDetection(fromMe, ipPacket);

            if (existedDetection != null)
            {
                this.OnReceived(ipPacket.DestinationAddress, ipPacket.SourceAddress, GetData(tcpPacket, udpPacket), fromMe,
                    existedDetection, port);

                existedDetection.DataTransfered += ipPacket.PayloadLength;
                if (ipPacket.PayloadLength > 32 || !this.HideSmallPackets)
                {
                    existedDetection.PackagesReceived++;
                }
                return;
            }

            var ro = this.CreateNewResultObject(ipPacket, fromMe);
            this.OnReceived(ipPacket.DestinationAddress, ipPacket.SourceAddress, GetData(tcpPacket, udpPacket), fromMe,
                ro, port);



            if (this.TryGetAccessToCollection().Any(x => x.ShowIp == ro.ShowIp))
            {
                return;
            }


            await this.AddToResult(ro);

            this.PostProcess(ro);
        }

        private IReadOnlyCollection<ResultObject> TryGetAccessToCollection()
        {
            while (this._collectionAccessWrite > 0)
            {
                Task.Delay(1);
            }

            var res = this.ResultObjects.ToArray();
            return res;
        }

        private async Task AddToResult(ResultObject ro)
        {
            if (this.InvokedAdd != null)
            {
                this.InvokedAdd(ro);
                await Task.Delay(0);
            }
            else
            {
                if (this.TryGetAccessToCollection().All(x => x.ShowIp != ro.ShowIp)) this.ResultObjects.Add(ro);
            }
        }

        private void PostProcess(ResultObject ro)
        {
            GeoWorker.InsertGeoDataQueue(ro);
            DnsHost.FillIpHost(ro, this._minimizeHostname);
            if (this.BlockNew)
            {
                Block.DoInSecBlock(ro);
            }
        }

        private ResultObject CreateNewResultObject(IpPacket ipPacket, bool fromMe)
        {
            var ro = new ResultObject
            {
                SourceAddress = ipPacket.SourceAddress,
                DestinationAddress = ipPacket.DestinationAddress,
                From = fromMe,
                PackagesReceived = 1,
                Application = this.SelectedAppEntry,
                DataTransfered = ipPacket.PayloadLength,
                ForceShow = this._forceShowHiddenIps
            };
            ro.Hidden = CustomSettings.Instance.CheckIpHidden(this.SelectedAppEntry.Name, ro.ShowIp);
            return ro;
        }

        private ResultObject GetExistedDetection(bool fromMe, IpPacket ipPacket)
        {
            ResultObject detection;

            try
            {
                detection = (fromMe
                    ? this.TryGetAccessToCollection().Where(
                        x =>
                            Equals(x.SourceAddress, ipPacket.DestinationAddress) ||
                            Equals(x.DestinationAddress, ipPacket.DestinationAddress))
                    : this.TryGetAccessToCollection().Where(
                        x =>
                            Equals(x.DestinationAddress, ipPacket.SourceAddress) ||
                            Equals(x.SourceAddress, ipPacket.SourceAddress))).FirstOrDefault();
            }
            catch (Exception)
            {
                Debugger.Log(1, "", "Get RO crushed.");
                detection = this.GetExistedDetection(fromMe, ipPacket);
            }

            return detection;
        }

        private void OnReceived(IPAddress ipdest, IPAddress ipsource, byte[] data, bool direction,
            ResultObject resultobject, IEnumerable<Tuple<Protocol, ushort>> protocol)
        {
            this.Received?.Invoke(ipdest, ipsource, data, direction, resultobject, protocol);
        }
    }
}