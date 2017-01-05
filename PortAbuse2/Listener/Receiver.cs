using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using PortAbuse2.Applications;
using PortAbuse2.Core.Common;
using PortAbuse2.Core.Geo;
using PortAbuse2.Core.Parser;
using PortAbuse2.Core.Proto;
using PortAbuse2.Core.Result;
using PortAbuse2.Core.WindowsFirewall;

namespace PortAbuse2.Listener
{
    internal class Receiver
    {
        private readonly bool _debug;
        public ObservableCollection<ResultObject> ResultObjects = new ObservableCollection<ResultObject>();
        private byte[] _byteData = new byte[65536];
        public bool ContinueCapturing; //A flag to check if packets are to be captured or not
        public AppIconEntry SelectedAppEntry;
        public string InterfaceLocalIp;
        public Socket MainSocket; //The socket which captures all incoming packets
        private readonly Window _window;
        private readonly string _logFolder = "raw";

        public bool BlockNew = false;

        public Receiver(Window window)
        {
            _window = window;
#if DEBUG
            _debug = true;
#endif
        }

        public void Clear()
        {
            ResultObjects.Clear();
        }

        public void StartListener(string ipInterface)
        {
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
                    MessageBox.Show(sEx.Message, "PortAbuse - Listener Error");
            }
            //Start receiving the packets asynchronously
            MainSocket.BeginReceive(_byteData, 0, _byteData.Length, SocketFlags.None, OnReceive, null);
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
                    MessageBox.Show(ex.Message, "PortAbuse - Receive Error");
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

            var msgBytes = ipHeader.Data.Take(ipHeader.MessageLength).ToArray();
            var msg = Encoding.Default.GetString(msgBytes);

            Encoding iso = Encoding.GetEncoding("ISO-8859-1");
            //byte[] isoBytes = Encoding.Convert(anscii, iso, msgBytes);
            string isoMsg = iso.GetString(msgBytes);
            string isoMsgUtf8 = Encoding.UTF8.GetString(msgBytes);
            //Thread safe adding of the nodes
            if (existedDetection != null)
            {
                if (ipHeader.MessageLength > 43)
                {
                    //if (_debug)
                    //    FileAccess.AppendFile(_logFolder, $"{existedDetection.ShowIp}.dump",
                    //        $"str:{msg}\r\n" +
                    //        $"isoStr:{isoMsg}\r\n" +
                    //        $"utfStr:{isoMsgUtf8}\r\n" +
                    //        $"byte:{BitConverter.ToString(ipHeader.Data.Take(ipHeader.MessageLength).ToArray())}\r\n---\r\n");
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
                Application = SelectedAppEntry
            };
            ro.Hidden = IpHider.Check(SelectedAppEntry.Name, ro.ShowIp);
            //if (_debug)
            //    FileAccess.AppendFile(_logFolder, $"{ro.ShowIp}.dump",
            //        $"str:{msg}\r\n" +
            //        $"isoStr:{isoMsg}\r\n" +
            //        $"utfStr:{isoMsgUtf8}\r\n" +
            //        $"byte:{BitConverter.ToString(ipHeader.Data.Take(ipHeader.MessageLength).ToArray())}\r\n---\r\n");

            if (ResultObjects.Any(x => x.ShowIp == ro.ShowIp))
                return;

            await _window.Dispatcher.BeginInvoke(new ThreadStart(delegate { ResultObjects.Add(ro); }));
            GeoWorker.InsertGeoDataQueue(ro);
            DnsHost.FillIpHost(ro);
            if (BlockNew)
            {
                Block.DoInSecBlock(ro);
            }
        }
    }
}