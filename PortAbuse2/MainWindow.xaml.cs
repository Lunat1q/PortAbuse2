using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MahApps.Metro.Controls;
using PortAbuse2.Applications;
using PortAbuse2.Core.Common;
using PortAbuse2.Core.Ip;
using PortAbuse2.Core.Result;
using PortAbuse2.Core.WindowsFirewall;
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
        private readonly Receiver _receiver;
        
        public MainWindow()
        {
            _receiver = new Receiver(this);
            InitializeComponent();

            Admin.CheckAdmin();

            IpHider.Load();

            LoadInterfaces();

            AppListComboBox.ItemsSource = _allAppWithPorts;

            ResultBox.ItemsSource = _receiver.ResultObjects;

            Task.Run(RefreshLoadProceses);
            //FillDummyData();
        }

        private void FillDummyData()
        {
            _receiver.ResultObjects.Add(new ResultObject
            {
                SourceAddress = new IPAddress(new byte[]{100,100,100,100}),
                DestinationAddress = new IPAddress(new byte[] { 100, 100, 100, 100 }),
                Hostname = "Test1"
            });

            _receiver.ResultObjects.Add(new ResultObject
            {
                SourceAddress = new IPAddress(new byte[] { 101, 100, 100, 100 }),
                DestinationAddress = new IPAddress(new byte[] { 101, 100, 100, 100 }),
                Hostname = "Test1"
            });
            _receiver.ResultObjects.Add(new ResultObject
            {
                SourceAddress = new IPAddress(new byte[] { 101, 100, 100, 100 }),
                DestinationAddress = new IPAddress(new byte[] { 101, 100, 100, 100 }),
                Hostname = "Test1"
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

            if (_receiver.SelectedAppEntry == null)
            {
                var wfApp = _allAppWithPorts.FirstOrDefault(x => x.Name.ToLower().Contains("warframe"));
                if (wfApp != null)
                {
                    AppListComboBox.SelectedItem = wfApp;
                }
            }

            if (btn == null || _receiver.SelectedAppEntry == null) return;
            if (!_receiver.ContinueCapturing)
            {
                _receiver.Clear();
                _receiver.ContinueCapturing = true;
                btn.Content = "Stop";
                _receiver.StartListener(InterfaceBox.SelectedItem.ToString());
                var red = FindResource("PaLightRed") as SolidColorBrush;
                btn.Background = red;
            }
            else
            {
                btn.Content = "Start";
                _receiver.ContinueCapturing = false;
                var green = FindResource("PaLightGreen") as SolidColorBrush;
                btn.Background = green;
            }
        }
       
        
        private async void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            IpHider.Save();
            await Block.Wait();
        }

        private void AppListComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cb = sender as ComboBox;
            var app = cb?.SelectedItem as AppIconEntry;
            if (app == null) return;
            _receiver.SelectedAppEntry = app;
        }

        private void BlockNewSwitch_Click(object sender, RoutedEventArgs e)
        {
            var tgl = sender as ToggleSwitch;
            if (tgl?.IsChecked != null)
                _receiver.BlockNew = (bool)tgl.IsChecked;
        }
    }
}
