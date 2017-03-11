using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using PortAbuse2.Core.Common;
using PortAbuse2.Core.Geo;
using PortAbuse2.Core.Parser;
using PortAbuse2.Core.Proto;
using PortAbuse2.Core.Result;
using PortAbuse2.Core.WindowsFirewall;

namespace PortAbuse2.Core.Listener
{
    public class CoreReceiver
    {
        private readonly bool _debug;
        private bool _hideOld;
        public ObservableCollection<ResultObject> ResultObjects = new ObservableCollection<ResultObject>();
        private byte[] _byteData = new byte[65536];
        public bool ContinueCapturing; //A flag to check if packets are to be captured or not
        public AppEntry SelectedAppEntry;
        public string InterfaceLocalIp;
        public Socket MainSocket; //The socket which captures all incoming packets
        //private readonly string _logFolder = "raw";
        public int OldTimeLimitSeconds = 120;
        public bool MinimizeHostname;

        public delegate void MessageDetectedEventHandler(
            IPAddress ipDest,
            IPAddress ipSource,
            byte[] data,
            bool direction,
            ResultObject resultObject,
            IEnumerable<Tuple<string, string>> protocol
        );

        public event MessageDetectedEventHandler Received;

        public Func<ResultObject, bool> InvokedAdd { get; set; }

        public bool BlockNew = false;
        public bool HideSmallPackets;
        private bool _forceShowHiddenIps;

        public CoreReceiver(bool minimizeHostname = false, bool hideOld = false, bool hideSmall = false)
        {
            MinimizeHostname = minimizeHostname;
            _hideOld = hideOld;
            HideSmallPackets = hideSmall;
#if DEBUG
            _debug = true;
#endif
        }

        public void HideOld()
        {
            _hideOld = true;
        }

        public void ShowOld()
        {
            _hideOld = false;
            Task.Delay(200);
            foreach (var ro in ResultObjects)
            {
                ro.Old = false;
            }
        }

        public void MinimizeHostnames()
        {
            MinimizeHostname = true;
            Task.Delay(200);
            foreach (var ro in ResultObjects)
            {
                ro.Hostname = DnsHost.MinimizeHostname(ro.DetectedHostname);
            }
        }

        public void SetForceShowHiddenIps(bool forceShow = true)
        {
            _forceShowHiddenIps = forceShow;
            Task.Delay(200);
            foreach (var ro in ResultObjects)
            {
                ro.ForceShow = _forceShowHiddenIps;
            }
        }

        public void UnminimizeHostnames()
        {
            MinimizeHostname = false;
            Task.Delay(200);
            foreach (var ro in ResultObjects)
            {
                ro.Hostname = ro.DetectedHostname;
            }
        }

        private async Task CleanupDupes() //TODO: Test if its needed at all
        {
            while (ContinueCapturing)
            {
                var dupes = ResultObjects.GroupBy(x => x.ShowIp).Where(x => x.Count() > 1);
                foreach (var dupe in dupes)
                {
                    if (dupe.Count() <= 1) continue;

                    var main = dupe.FirstOrDefault();

                    if (main == null) continue;

                    foreach (var second in dupe.Where(x=>x != main))
                    {
                        main.PackagesReceived += second.PackagesReceived;
                        ResultObjects.Remove(second);
                    }
                }
                await Task.Delay(5000);
            }
        }

        private async Task HideOldTask()
        {
            while (ContinueCapturing)
            {
                if (_hideOld)
                {
                    var nowMinusShift = DateTime.UtcNow.ToUnixTime() - OldTimeLimitSeconds*1000;
                    foreach (var ro in ResultObjects)
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
            ResultObjects.Clear();
        }

        public void StartListener(string ipInterface)
        {
            Task.Run(HideOldTask);
            Task.Run(CleanupDupes);

            InterfaceLocalIp = ipInterface;
            var ipe = new IPEndPoint(IPAddress.Parse(ipInterface), 0);

            //mainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);
            MainSocket = new Socket(ipe.AddressFamily, SocketType.Raw, ProtocolType.IP);

            //Bind the socket to the selected IP address  
            MainSocket.Bind(ipe);
            //Set the socket  options
            MainSocket.SetSocketOption(SocketOptionLevel.IP, //Applies only to IP packets
                SocketOptionName.HeaderIncluded, //Set the include the header
                true); //option to true

            byte[] byTrue = {1, 0, 0, 0};
            byte[] byOut = {1, 0, 0, 0}; //Capture outgoing packets
            //Socket.IOControl is analogous to the WSAIoctl method of Winsock 2
            try
            {
                MainSocket.IOControl(IOControlCode.ReceiveAll, byTrue, byOut);
                //Equivalent to SIO_RCVALL constant   //of Winsock 2                                             
            }
            catch (SocketException sEx)
            {
                if (_debug)
                    Debug.WriteLine("StartListener Err:" + sEx.Message);
                    //MessageBox.Show(sEx.Message, "PortAbuse - Listener Error");
            }
            //Start receiving the packets asynchronously
            MainSocket.BeginReceive(_byteData, 0, _byteData.Length, SocketFlags.None, OnReceive, null);
        }

        public void SendToUdp(IPAddress ip, byte[] data, int port)
        {
            var remoteEndPoint = new IPEndPoint(ip, port);
            var server = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Udp);
            server.Connect(remoteEndPoint);
            server.SendTo(data, data.Length, SocketFlags.None, remoteEndPoint);
        }

        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                var nReceived = MainSocket.EndReceive(ar);
                if (nReceived > 65536) nReceived = 65536;
                Task.Run(() => ParseData(_byteData, nReceived));

                if (!ContinueCapturing) return;
                _byteData = new byte[65536];

                //Another call to BeginReceive so that we continue to receive the incoming
                //packets
                MainSocket.BeginReceive(_byteData, 0, _byteData.Length, SocketFlags.None, OnReceive, null);
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                if (_debug)
                    Debug.WriteLine("OnReceive Err:" + ex.Message);
            }
        }

        private async Task ParseData(byte[] receivedByteData, int nReceived)
        {
            //Since all protocol packets are encapsulated in the IP datagram
            //so we start by parsing the IP header and see what protocol data
            //is being carried by it
            var ipHeader = new IpHeader(receivedByteData, nReceived);

            //Now according to the protocol being carried by the IP datagram we parse 
            //the data field of the datagram
            var port = Package.GetPorts(ipHeader);

            var portsMatch =
                SelectedAppEntry.AppPort.Any(
                    x => port != null && port.Any(v => v.Item2 == x.PortNumber && x.Protocol == v.Item1));
            if (!portsMatch) return;

            var fromMe = ipHeader.SourceAddress.ToString() == InterfaceLocalIp;
            var existedDetection = (fromMe
                ? ResultObjects.Where(
                    x =>
                        Equals(x.SourceAddress, ipHeader.DestinationAddress) ||
                        Equals(x.DestinationAddress, ipHeader.DestinationAddress))
                : ResultObjects.Where(
                    x =>
                        Equals(x.DestinationAddress, ipHeader.SourceAddress) ||
                        Equals(x.SourceAddress, ipHeader.SourceAddress))).FirstOrDefault();
            
            if (existedDetection != null)
            {
                Received?.Invoke(
                    ipHeader.DestinationAddress,
                    ipHeader.SourceAddress,
                    ipHeader.Data.Take(ipHeader.MessageLength).ToArray(),
                    fromMe,
                    existedDetection,
                    port
                );

                existedDetection.DataTransfered += ipHeader.MessageLength;
                if (ipHeader.MessageLength > 32 || !HideSmallPackets)
                {
                    existedDetection.PackagesReceived++;
                }
                return;
            }
            var ro = new ResultObject
            {
                SourceAddress = ipHeader.SourceAddress,
                DestinationAddress = ipHeader.DestinationAddress,
                From = fromMe,
                PackagesReceived = 1,
                Application = SelectedAppEntry,
                DataTransfered = ipHeader.MessageLength,
                ForceShow = _forceShowHiddenIps
            };
            ro.Hidden = IpHider.Check(SelectedAppEntry.Name, ro.ShowIp);

            if (ResultObjects.Any(x => x.ShowIp == ro.ShowIp))
                return;

            if (InvokedAdd != null)
            {
                InvokedAdd(ro);
                await Task.Delay(0);
            }
            else
            {
                if (ResultObjects.All(x => x.ShowIp != ro.ShowIp))
                    ResultObjects.Add(ro);
            }
            GeoWorker.InsertGeoDataQueue(ro);
            DnsHost.FillIpHost(ro, MinimizeHostname);
            if (BlockNew)
            {
                Block.DoInSecBlock(ro);
            }
        }
    }
}