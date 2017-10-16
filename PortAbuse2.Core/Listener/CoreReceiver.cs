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

        private int _collectionAccessRead = 0;
        private int _collectionAccessWrite = 0;
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
            ContinueCapturing = false;
            _captureDevice?.StopCapture();
            if (_currentExtensions != null)
            {
                foreach (var ext in _currentExtensions)
                {
                    Received -= ext.PackageReceived;
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
            _minimizeHostname = minimizeHostname;
            _hideOld = hideOld;
            HideSmallPackets = hideSmall;
        }

        public void HideOld()
        {
            _hideOld = true;
        }

        public void ShowOld()
        {
            _hideOld = false;
            Task.Delay(RegularActionsDelay);
            foreach (var ro in TryGetAccessToCollection())
            {
                ro.Old = false;
            }
        }

        public void MinimizeHostnames()
        {
            _minimizeHostname = true;
            Task.Delay(RegularActionsDelay);
            foreach (var ro in TryGetAccessToCollection())
            {
                ro.Hostname = DnsHost.MinimizeHostname(ro.DetectedHostname);
            }
        }

        public void SetForceShowHiddenIps(bool forceShow = true)
        {
            _forceShowHiddenIps = forceShow;
            Task.Delay(RegularActionsDelay);
            foreach (var ro in TryGetAccessToCollection())
            {
                ro.ForceShow = _forceShowHiddenIps;
            }
        }

        public void UnminimizeHostnames()
        {
            _minimizeHostname = false;
            Task.Delay(RegularActionsDelay);
            foreach (var ro in TryGetAccessToCollection())
            {
                ro.Hostname = ro.DetectedHostname;
            }
        }

        private async Task CleanupDupes() //TODO: Test if its needed at all
        {
            while (ContinueCapturing)
            {
                StartWrite();
                var dupes = ResultObjects.GroupBy(x => x.ShowIp).Where(x => x.Count() > 1);
                foreach (var dupe in dupes)
                {
                    if (dupe.Count() <= 1) continue;

                    var main = dupe.FirstOrDefault();

                    if (main == null) continue;

                    foreach (var second in dupe.Where(x => x != main))
                    {
                        main.PackagesReceived += second.PackagesReceived;
                        ResultObjects.Remove(second);
                    }
                }
                EndWrite();
                await Task.Delay(5000);
            }
        }

        private async Task HideOldTask()
        {
            while (ContinueCapturing)
            {
                if (_hideOld)
                {
                    var nowMinusShift = DateTime.UtcNow.ToUnixTime() - OldTimeLimitSeconds * 1000;
                    foreach (var ro in TryGetAccessToCollection())
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
            StartWrite();
            ResultObjects.Clear();
            EndWrite();
        }

        private void EndWrite()
        {
            _collectionAccessWrite--;
        }

        private void StartWrite()
        {
            _collectionAccessWrite++;
        }

        private void InitExtensions()
        {
            _currentExtensions = ExtensionsRepository.GetExtensionsForApp(SelectedAppEntry.Name);
            foreach (var ext in _currentExtensions)
            {
                Received += ext.PackageReceived;
                ext.ResultObjectRef = ResultObjects;
                ext.Start();
            }
        }

        public void StartListener(string ipInterface)
        {
            ContinueCapturing = true;
            StartWrite();
            ResultObjects.Clear();
            EndWrite();
            Task.Run(HideOldTask);
            Task.Run(CleanupDupes);
            InitExtensions();


            _interfaceLocalIp = ipInterface;
            // метод для получения списка устройств
            CaptureDeviceList deviceList = CaptureDeviceList.Instance;
            foreach (var device in deviceList)
            {
                var pcapDevice = device as WinPcapDevice;
                if (pcapDevice == null) continue;
                var winDevice = pcapDevice;
                if (winDevice.Addresses.Any(x => x.Addr.ToString() == _interfaceLocalIp))
                {
                    _captureDevice = winDevice;
                    break;
                }
            }
            if (_captureDevice == null) //просрали девайс, берем первый и молимся
                _captureDevice = deviceList[0];
            // регистрируем событие, которое срабатывает, когда пришел новый пакет
            _captureDevice.OnPacketArrival += Receiver_OnPacketArrival;
            // открываем в режиме promiscuous, поддерживается также нормальный режим
            _captureDevice.Open(DeviceMode.Promiscuous, 1000);
            // начинаем захват пакетов
            _captureDevice.StartCapture();
        }

        private void Receiver_OnPacketArrival(object sender, CaptureEventArgs e)
        {
            // парсинг всего пакета
            try
            {
                var packet = Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);
                Task.Run(() => ParsePacket(packet));
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

            var portsMatch =
                SelectedAppEntry.AppPort.Any(
                    x => port != null && port.Any(v => v.Item2 == x.UPortNumber && x.Protocol == v.Item1));
            if (!portsMatch) return;

            var fromMe = ipPacket.SourceAddress.ToString() == _interfaceLocalIp;
            var existedDetection = GetExistedDetection(fromMe, ipPacket);

            if (existedDetection != null)
            {
                OnReceived(ipPacket.DestinationAddress, ipPacket.SourceAddress, GetData(tcpPacket, udpPacket), fromMe,
                    existedDetection, port);

                existedDetection.DataTransfered += ipPacket.PayloadLength;
                if (ipPacket.PayloadLength > 32 || !HideSmallPackets)
                {
                    existedDetection.PackagesReceived++;
                }
                return;
            }

            var ro = CreateNewResultObject(ipPacket, fromMe);
            OnReceived(ipPacket.DestinationAddress, ipPacket.SourceAddress, GetData(tcpPacket, udpPacket), fromMe,
                ro, port);



            if (TryGetAccessToCollection().Any(x => x.ShowIp == ro.ShowIp))
            {
                return;
            }


            await AddToResult(ro);

            PostProcess(ro);
        }

        private IReadOnlyCollection<ResultObject> TryGetAccessToCollection()
        {
            while (_collectionAccessWrite > 0)
            {
                Task.Delay(1);
            }
            _collectionAccessRead++;
            var res = ResultObjects.ToArray();
            _collectionAccessRead--;
            return res;
        }

        private async Task AddToResult(ResultObject ro)
        {
            if (InvokedAdd != null)
            {
                InvokedAdd(ro);
                await Task.Delay(0);
            }
            else
            {
                if (TryGetAccessToCollection().All(x => x.ShowIp != ro.ShowIp))
                    ResultObjects.Add(ro);
            }
        }

        private void PostProcess(ResultObject ro)
        {
            GeoWorker.InsertGeoDataQueue(ro);
            DnsHost.FillIpHost(ro, _minimizeHostname);
            if (BlockNew)
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
                Application = SelectedAppEntry,
                DataTransfered = ipPacket.PayloadLength,
                ForceShow = _forceShowHiddenIps
            };
            ro.Hidden = CustomSettings.Instance.CheckIpHidden(SelectedAppEntry.Name, ro.ShowIp);
            return ro;
        }

        private ResultObject GetExistedDetection(bool fromMe, IpPacket ipPacket)
        {
            ResultObject detection;

            try
            {
                detection = (fromMe
                    ? TryGetAccessToCollection().Where(
                        x =>
                            Equals(x.SourceAddress, ipPacket.DestinationAddress) ||
                            Equals(x.DestinationAddress, ipPacket.DestinationAddress))
                    : TryGetAccessToCollection().Where(
                        x =>
                            Equals(x.DestinationAddress, ipPacket.SourceAddress) ||
                            Equals(x.SourceAddress, ipPacket.SourceAddress))).FirstOrDefault();
            }
            catch (Exception)
            {
                Debugger.Log(1, "", "Get RO crushed.");
                detection = GetExistedDetection(fromMe, ipPacket);
            }

            return detection;
        }

        private void OnReceived(IPAddress ipdest, IPAddress ipsource, byte[] data, bool direction,
            ResultObject resultobject, IEnumerable<Tuple<Protocol, ushort>> protocol)
        {
            Received?.Invoke(ipdest, ipsource, data, direction, resultobject, protocol);
        }
    }
}