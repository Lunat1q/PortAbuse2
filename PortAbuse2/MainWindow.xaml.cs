using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PortAbuse2.Applications;
using PortAbuse2.Common;
using PortAbuse2.Core.Geo;
using PortAbuse2.Core.Ip;
using PortAbuse2.Core.Parser;
using PortAbuse2.Core.Proto;
using PortAbuse2.Core.Result;
using PortAbuse2.Core.WindowsFirewall;

namespace PortAbuse2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly ObservableCollection<AppEntry> _allAppWithPorts = new ObservableCollection<AppEntry>();
        private readonly ObservableCollection<ResultObject> _resultObjects = new ObservableCollection<ResultObject>();
        private Socket _mainSocket; //The socket which captures all incoming packets
        private byte[] _byteData = new byte[65536];
        private bool _bContinueCapturing; //A flag to check if packets are to be captured or not
        private AppEntry _selectedAppEntry;

        private bool _blockNew = false;

        private string _interfaceLocalIp;

        public MainWindow()
        {
            InitializeComponent();

            Admin.CheckAdmin();

            LoadInterfaces();

            AppListComboBox.ItemsSource = _allAppWithPorts;

            ResultBox.ItemsSource = _resultObjects;

            Task.Run(RefreshLoadProceses);
        }

        private async Task RefreshLoadProceses()
        {
            var apps = await AppList.GetRunningApplications();
            foreach (var p in apps)
            {
                if (_allAppWithPorts.All(x => x.InstancePid != p.InstancePid))
                {
                    await Dispatcher.BeginInvoke(new ThreadStart(delegate
                    {
                        p.Icon = AppIcon.GetIcon(p.FullName, false);
                        _allAppWithPorts.Add(p);
                    }));
                }
            }
        }

        private void LoadInterfaces()
        {
            var interfaces = IpInterface.GetIpInterfaces();
            var itemsSource = interfaces as string[] ?? interfaces.ToArray();
            InterfaceBox.ItemsSource = itemsSource;
            if (itemsSource.Length == 1)
            {
                InterfaceBox.SelectedIndex = 0;
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            _allAppWithPorts.Clear();
            Task.Run(RefreshLoadProceses);
        }

        private void SwitchButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null || _selectedAppEntry == null) return;
            if (!_bContinueCapturing)
            {
                _resultObjects.Clear();
                _bContinueCapturing = true;
                btn.Content = "Stop";
                StartListener(InterfaceBox.SelectedItem.ToString());
                var red = FindResource("PaLightRed") as SolidColorBrush;
                btn.Background = red;
            }
            else
            {
                btn.Content = "Start";
                _bContinueCapturing = false;
                var green = FindResource("PaLightGreen") as SolidColorBrush;
                btn.Background = green;
            }
        }

        private void StartListener(string ipInterface)
        {
            _interfaceLocalIp = ipInterface;
            var ipe = new IPEndPoint(IPAddress.Parse(ipInterface), 0);

            //mainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);
            _mainSocket = new Socket(ipe.AddressFamily, SocketType.Raw, ProtocolType.IP);

            //Bind the socket to the selected IP address  
            _mainSocket.Bind(ipe);
            //Set the socket  options
            _mainSocket.SetSocketOption(SocketOptionLevel.IP, //Applies only to IP packets
                SocketOptionName.HeaderIncluded, //Set the include the header
                true); //option to true

            byte[] byTrue = {1, 0, 0, 0};
            byte[] byOut = {1, 0, 0, 0}; //Capture outgoing packets
            //Socket.IOControl is analogous to the WSAIoctl method of Winsock 2
            try
            {
                _mainSocket.IOControl(IOControlCode.ReceiveAll, byTrue, byOut);
                //Equivalent to SIO_RCVALL constant   //of Winsock 2                                             
            }
            catch (SocketException sEx)
            {
                Title = "PortAbuse - Sniffer" + " [ERR:" + sEx.ErrorCode + "]";
            }
            //Start receiving the packets asynchronously
            _mainSocket.BeginReceive(_byteData, 0, _byteData.Length, SocketFlags.None, OnReceive, null);
        }

        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                var nReceived = _mainSocket.EndReceive(ar);
                if (nReceived > 65536) nReceived = 65536;
                ParseData(_byteData, nReceived);

                if (_bContinueCapturing)
                {
                    _byteData = new byte[65536];

                    //Another call to BeginReceive so that we continue to receive the incoming
                    //packets
                    _mainSocket.BeginReceive(_byteData, 0, _byteData.Length, SocketFlags.None, OnReceive, null);
                }
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "PortAbuse - Receive Error");
            }
        }

        private void ParseData(byte[] receivedByteData, int nReceived)
        {
            //Since all protocol packets are encapsulated in the IP datagram
            //so we start by parsing the IP header and see what protocol data
            //is being carried by it
            var ipHeader = new IpHeader(receivedByteData, nReceived);

            //Now according to the protocol being carried by the IP datagram we parse 
            //the data field of the datagram
            var port = Package.GetPorts(ipHeader);

            var portsMatch =
                _selectedAppEntry.AppPort.Any(x => port.Any(v => v.Item2 == x.PortNumber && x.Protocol == v.Item1));
            if (!portsMatch) return;

            var newDetection = !_resultObjects.Any(
                x =>
                    Equals(x.SourceAddress, ipHeader.SourceAddress) &&
                    Equals(x.DestinationAddress, ipHeader.DestinationAddress));

            //Thread safe adding of the nodes
            if (newDetection)
            {
                var ro = new ResultObject()
                {
                    SourceAddress = ipHeader.SourceAddress,
                    DestinationAddress = ipHeader.DestinationAddress,
                    Reverse = ipHeader.SourceAddress.ToString() == _interfaceLocalIp
                };

                Dispatcher.BeginInvoke(new ThreadStart(delegate { _resultObjects.Add(ro); }));
                GeoWorker.InsertGeoDataQueue(ro);
                DnsHost.FillIpHost(ro);
                if (_blockNew)
                {
                    Block.Do30SecBlock(ro);
                }
            }
        }

       

        private async void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            await Block.Wait();
        }

        private void AppListComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cb = sender as ComboBox;
            var app = cb?.SelectedItem as AppEntry;
            if (app == null) return;
            _selectedAppEntry = app;
        }
    }
}
