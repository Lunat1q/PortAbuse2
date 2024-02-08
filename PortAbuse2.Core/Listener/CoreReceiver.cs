using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using PacketDotNet;
using PortAbuse2.Core.ApplicationExtensions;
using PortAbuse2.Core.Common;
using PortAbuse2.Core.Geo;
using PortAbuse2.Core.Ip;
using PortAbuse2.Core.Parser;
using PortAbuse2.Core.Port;
using PortAbuse2.Core.Result;
using PortAbuse2.Core.WindowsFirewall;
using SharpPcap;

namespace PortAbuse2.Core.Listener
{
    public class CoreReceiver
    {
        private bool _hideOld;

        // ReSharper disable once FieldCanBeMadeReadOnly.Global

        private readonly IResultReceiver _resultReceiver;

        private readonly ConcurrentDictionary<IPAddress, PushableInfo> _resultDictionary = new ConcurrentDictionary<IPAddress, PushableInfo>();
        private IEnumerable<IApplicationExtension> _currentExtensions;
        private readonly CaptureDevicesInfo _captureDevices = new CaptureDevicesInfo();

        //private readonly string _logFolder = "raw";
        private const int OldTimeLimitSeconds = 30;

        private const int RegularActionsDelay = 500;

        private bool _minimizeHostname;

        public delegate void MessageDetectedEventHandler(
            IPAddress ipDest,
            IPAddress ipSource,
            byte[] data,
            bool direction,
            ConnectionInformation connectionInformation,
            PortInformation portInfo
        );

        protected CoreReceiver(IResultReceiver receiver, bool minimizeHostname = false, bool hideOld = false, bool hideSmall = false)
        {
            this._resultReceiver = receiver;
            this._minimizeHostname = minimizeHostname;
            this._hideOld = hideOld;
            this.HideSmallPackets = hideSmall;
        }
        /// <summary>
        /// A flag to check if packets are to be captured or not
        /// </summary>
        public bool ContinueCapturing { get; private set; }

        public AppEntry SelectedAppEntry
        {
            get => this._selectedAppEntry;
            set
            {
                this._selectedAppEntry = value;
                this.UpdatePortStorage();
            }
        }

        private void UpdatePortStorage()
        {
            this._portStorage = new PortStorage(this._selectedAppEntry.AppPort);
        }

        public void Stop()
        {
            this.ContinueCapturing = false;
            foreach (var device in this._captureDevices)
            {
                device.StopCapture();
                device.Close();
            }
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

        protected Func<ConnectionInformation, Task<bool>> InvokedAdd { get; set; }

        public bool BlockNew = false;
        public bool HideSmallPackets;
        private bool _forceShowHiddenIps;
        private AppEntry _selectedAppEntry;
        private PortStorage _portStorage;


        public void HideOld()
        {
            this._hideOld = true;
        }

        public void ShowOld()
        {
            this._hideOld = false;
            Task.Delay(RegularActionsDelay);
            foreach (var ro in this._resultDictionary.Values)
            {
                ro.Data.Old = false;
            }
        }

        public void MinimizeHostnames()
        {
            this._minimizeHostname = true;
            Task.Delay(RegularActionsDelay);
            foreach (var ro in this._resultDictionary.Values)
            {
                ro.Data.Hostname = DnsHost.MinimizeHostname(ro.Data.DetectedHostname);
            }
        }

        public void SetForceShowHiddenIps(bool forceShow = true)
        {
            this._forceShowHiddenIps = forceShow;
            Task.Delay(RegularActionsDelay);
            foreach (var ro in this._resultDictionary.Values)
            {
                ro.Data.ForceShow = this._forceShowHiddenIps;
            }
        }

        public void MaximizeHostnames()
        {
            this._minimizeHostname = false;
            Task.Delay(RegularActionsDelay);
            foreach (var ro in this._resultDictionary.Values)
            {
                ro.Data.Hostname = ro.Data.DetectedHostname;
            }
        }

        private async Task PushNewToReceiver()
        {
            while (this.ContinueCapturing)
            {
                var newObjects = this._resultDictionary.Where(x => !x.Value.IsPushed);

                foreach (var n in newObjects)
                {
                    n.Value.IsPushed = true;
                    await this.InvokedAdd(n.Value.Data);
                }

                await Task.Delay(500);
            }
        }

        private async Task HideOldTask()
        {
            while (this.ContinueCapturing)
            {
                if (this._hideOld)
                {
                    var nowMinusShift = DateTime.UtcNow.ToUnixTime() - OldTimeLimitSeconds * 1000;
                    foreach (var ro in this._resultDictionary.Values.Select(x => x.Data))
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
            this._resultReceiver.Reset();
            this._resultDictionary.Clear();
        }
        
        private void InitExtensions()
        {
            this._currentExtensions = ExtensionsRepository.GetExtensionsForApp(this.SelectedAppEntry.Name);
            foreach (var ext in this._currentExtensions)
            {
                this.Received += ext.PackageReceived;
                ext.Start();
            }
        }

        public void StartListener(IpInterface selectedIpInterface)
        {
            this.ContinueCapturing = true;
            this.Clear();
            Task.Run(this.HideOldTask);
            Task.Run(this.PushNewToReceiver);
            Task.Run(this.UpdateConnectionsNumber);
            this.InitExtensions();


            this._captureDevices.SelectedCaptureDevice(selectedIpInterface);

            foreach (var device in this._captureDevices)
            {
                device.OnPacketArrival += this.Receiver_OnPacketArrival;
                device.Open(new DeviceConfiguration
                {
                    ReadTimeout = 1000,
                    Mode = DeviceModes.None
                });
                device.StartCapture();
            }

        }

        private async Task UpdateConnectionsNumber()
        {
            var processIds = new HashSet<int> {this.SelectedAppEntry.InstancePid};
            while (this.ContinueCapturing)
            {
                var tcpConnectionsForProcess = SocketConnectionsReader.GetActiveTcpConnectionsForProcess(processIds);
                var udpConnectionsForProcess = SocketConnectionsReader.GetActiveUdpConnectionsForProcess(processIds);
                this.SelectedAppEntry.TcpConnections = tcpConnectionsForProcess.Count();
                this.SelectedAppEntry.UdpConnections = udpConnectionsForProcess.Count();
                await Task.Delay(10000);
            }
        }

        private void Receiver_OnPacketArrival(object sender, PacketCapture e)
        {
            try
            {
                var raw = e.GetPacket();
                var packet = Packet.ParsePacket(raw.LinkLayerType, raw.Data);
                Task.Run(() => this.ParsePacket(packet));
            }
            catch
            {
                // ignored
            }
        }

        private static byte[] GetData(Packet tcp, Packet udp)
        {
            return tcp?.PayloadData ?? udp.PayloadData;
        }

        private void ParsePacket(Packet packet)
        {
            var port = PackageHelper.GetPorts(packet, out var ipPacket, out var tcpPacket,
                out var udpPacket);

            var portsMatch = this._portStorage.Any(port);
            if (!portsMatch) return;

            var fromMe = this.IsPacketSentFromMe(packet, ipPacket);
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

            var ro = ConnectionInformation.CreateNewResultObject(ipPacket, fromMe, this.SelectedAppEntry, this._forceShowHiddenIps);
            this.OnReceived(ipPacket.DestinationAddress, ipPacket.SourceAddress, GetData(tcpPacket, udpPacket), fromMe,
                ro, port);

            this.AddToResult(ro);

            this.PostProcess(ro);
        }

        private bool IsPacketSentFromMe(Packet packet, IPPacket ipPacket)
        {
            if (packet is EthernetPacket ethPacket)
            {
                if (this._captureDevices.HavePhysicalAddressesInitialized())
                {
                    return this._captureDevices.IsSelectedDeviceAddress(ethPacket.SourceHardwareAddress);
                }

                return this._captureDevices.IsVirtualMac(ethPacket.SourceHardwareAddress) && ipPacket.TimeToLive % 64 == 0;
            }

            return ipPacket.TimeToLive % 64 == 0;
        }


        private void AddToResult(ConnectionInformation ro)
        {
            this._resultDictionary.TryAdd(ro.ShowIp, new PushableInfo(ro));
        }

        private void PostProcess(ConnectionInformation ro)
        {
            GeoWorker.InsertGeoDataQueue(ro);
            DnsHost.FillIpHost(ro, this._minimizeHostname);
            if (this.BlockNew)
            {
                Block.DoInSecBlock(ro);
            }
        }

        private ConnectionInformation GetExistedDetection(bool fromMe, IPPacket ipPacket)
        {
            var showIp = fromMe ? ipPacket.DestinationAddress : ipPacket.SourceAddress;

            try
            {
                if (this._resultDictionary.TryGetValue(showIp, out var result))
                {
                    return result.Data;
                }
            }
            catch (Exception)
            {
                Debugger.Log(1, "", "Get RO crushed.");
                return this.GetExistedDetection(fromMe, ipPacket);
            }

            return null;
        }

        private void OnReceived(IPAddress ipDest, IPAddress ipSource, byte[] data, bool direction,
            ConnectionInformation resultObject, PortInformation portInfo)
        {
            this.Received?.Invoke(ipDest, ipSource, data, direction, resultObject, portInfo);
        }

        private class PushableInfo
        {
            public PushableInfo(ConnectionInformation ro)
            {
                this.Data = ro;
            }

            public bool IsPushed { get; set; }

            public ConnectionInformation Data { get; }
        }
    }
}