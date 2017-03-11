using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PortAbuse2.Applications;
using PortAbuse2.Common;
using PortAbuse2.Controls;
using PortAbuse2.Core.Common;
using PortAbuse2.Core.Ip;
using PortAbuse2.Core.Result;
using PortAbuse2.Core.WindowsFirewall;
using PortAbuse2.KeyCapture;
using PortAbuse2.Listener;
using Admin = PortAbuse2.Common.Admin;

namespace PortAbuse2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly ObservableCollection<AppIconEntry> _allAppWithPorts = new ObservableCollection<AppIconEntry>();
        internal readonly Receiver Receiver;
        private readonly ExtensionControl _extensionControl;
        private readonly KeyEventsHandling _keyEventsHandling;

        public MainWindow()
        {
            Receiver = new Receiver(this, Properties.Settings.Default.MinimizeHostname,
                Properties.Settings.Default.HideOldConnections,
                Properties.Settings.Default.HideSmallPackets);

            _extensionControl = new ExtensionControl(Receiver);
            InitializeComponent();

            //"hacky" fix of designer glitch
            FlyOutGrid.Visibility = Visibility.Visible;

            Admin.CheckAdmin();

            IpHider.Load();

            LoadInterfaces();

            AppListComboBox.ItemsSource = _allAppWithPorts;

            ResultBox.ItemsSource = Receiver.ResultObjects;

            Task.Run(RefreshLoadProceses);

            SettingPage.SetMainWindow(this);

            _keyEventsHandling = new KeyEventsHandling();

#if DEBUG
            FillDummyData();
#endif
        }

        private void FillDummyData()
        {
            Receiver.ResultObjects.Add(new ResultObject
            {
                SourceAddress = new IPAddress(new byte[]{100,100,100,100}),
                DestinationAddress = new IPAddress(new byte[] { 100, 100, 100, 100 }),
                Hostname = "Test1",
                PackagesReceived = 662
            });

            Receiver.ResultObjects.Add(new ResultObject
            {
                SourceAddress = new IPAddress(new byte[] { 101, 100, 100, 100 }),
                DestinationAddress = new IPAddress(new byte[] { 101, 100, 100, 100 }),
                Hostname = "Test1",
                PackagesReceived = 32567
            });
            Receiver.ResultObjects.Add(new ResultObject
            {
                SourceAddress = new IPAddress(new byte[] { 102, 100, 100, 100 }),
                DestinationAddress = new IPAddress(new byte[] { 102, 100, 100, 100 }),
                Hostname = "Test1",
                PackagesReceived = 1000000000
            });
            Receiver.ResultObjects.Add(new ResultObject
            {
                SourceAddress = new IPAddress(new byte[] { 103, 100, 100, 100 }),
                DestinationAddress = new IPAddress(new byte[] { 103, 100, 100, 100 }),
                Hostname = "Test1",
                PackagesReceived = 3756412
            });
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

        public void RemapBlockButtons(int amount)
        {
            BlockTimeContainer.CurrentBlockTime = amount;
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

            if (Receiver.SelectedAppEntry == null)
            {
                var wfApp = _allAppWithPorts.FirstOrDefault(x => x.Name.ToLower().Contains("warframe"));
                if (wfApp != null)
                {
                    AppListComboBox.SelectedItem = wfApp;
                }
            }

            if (btn == null || Receiver.SelectedAppEntry == null) return;
            if (!Receiver.ContinueCapturing)
            {
                Receiver.Clear();
                Receiver.ContinueCapturing = true;
                btn.Content = "Stop";
                Receiver.StartListener(InterfaceBox.SelectedItem.ToString());
                var red = FindResource("PaLightRed") as SolidColorBrush;
                btn.Background = red;
            }
            else
            {
                btn.Content = "Start";
                Receiver.ContinueCapturing = false;
                var green = FindResource("PaLightGreen") as SolidColorBrush;
                btn.Background = green;
            }
        }
       
        
        private async void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            IpHider.Save();
            _extensionControl.Stop();
            _keyEventsHandling.Stop();
            await Block.Wait();
        }

        private void AppListComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cb = sender as ComboBox;
            var app = cb?.SelectedItem as AppIconEntry;
            if (app == null) return;
            Receiver.SelectedAppEntry = app;
            _extensionControl.AppSelected(app.Name);
        }



        private void button_Click(object sender, RoutedEventArgs e)
        {
            ExtraOptions.IsOpen = !ExtraOptions.IsOpen;
        }
    }
}
