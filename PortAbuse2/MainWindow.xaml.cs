﻿using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MahApps.Metro.Controls;
using PortAbuse2.Applications;
using PortAbuse2.Common;
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
        private readonly ExtensionControl _extensionControl;
        
        public MainWindow()
        {
            _receiver = new Receiver(this, Properties.Settings.Default.MinimizeHostname,
                Properties.Settings.Default.HideOldConnections,
                Properties.Settings.Default.HideSmallPackets);

            _extensionControl = new ExtensionControl(_receiver);
            InitializeComponent();

            Admin.CheckAdmin();

            IpHider.Load();

            LoadInterfaces();

            AppListComboBox.ItemsSource = _allAppWithPorts;

            ResultBox.ItemsSource = _receiver.ResultObjects;

            Task.Run(RefreshLoadProceses);

            LoadSettings();

#if DEBUG
            FillDummyData();
#endif
        }

        private void LoadSettings()
        {
            MinimizeHostnames.IsChecked = Properties.Settings.Default.MinimizeHostname;
            HideOldRecords.IsChecked = Properties.Settings.Default.HideOldConnections;
            HideSmallPackets.IsChecked = Properties.Settings.Default.HideSmallPackets;

            VersionNumberBlock.Text = $"v{Assembly.GetExecutingAssembly().GetName().Version}";
        }

        private void FillDummyData()
        {
            _receiver.ResultObjects.Add(new ResultObject
            {
                SourceAddress = new IPAddress(new byte[]{100,100,100,100}),
                DestinationAddress = new IPAddress(new byte[] { 100, 100, 100, 100 }),
                Hostname = "Test1",
                PackagesReceived = 662
            });

            _receiver.ResultObjects.Add(new ResultObject
            {
                SourceAddress = new IPAddress(new byte[] { 101, 100, 100, 100 }),
                DestinationAddress = new IPAddress(new byte[] { 101, 100, 100, 100 }),
                Hostname = "Test1",
                PackagesReceived = 32567
            });
            _receiver.ResultObjects.Add(new ResultObject
            {
                SourceAddress = new IPAddress(new byte[] { 102, 100, 100, 100 }),
                DestinationAddress = new IPAddress(new byte[] { 102, 100, 100, 100 }),
                Hostname = "Test1",
                PackagesReceived = 1000000000
            });
            _receiver.ResultObjects.Add(new ResultObject
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
            _extensionControl.Stop();
            await Block.Wait();
        }

        private void AppListComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cb = sender as ComboBox;
            var app = cb?.SelectedItem as AppIconEntry;
            if (app == null) return;
            _receiver.SelectedAppEntry = app;
            _extensionControl.AppSelected(app.Name);
        }

        private void BlockNewSwitch_Click(object sender, RoutedEventArgs e)
        {
            var tgl = sender as ToggleSwitch;
            if (tgl?.IsChecked != null)
                _receiver.BlockNew = (bool)tgl.IsChecked;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            ExtraOptions.IsOpen = !ExtraOptions.IsOpen;
        }

        private void HideOldRecords_OnClickSwitch_Click(object sender, RoutedEventArgs e)
        {
            var tgl = sender as ToggleSwitch;
            if (tgl?.IsChecked == null) return;

            Properties.Settings.Default.HideOldConnections = (bool)tgl.IsChecked;
            Properties.Settings.Default.Save();

            if ((bool) tgl.IsChecked)
                _receiver.HideOld();
            else
                _receiver.ShowOld();
        }

        private void MinimizeHostnames_OnClickSwitch_Click(object sender, RoutedEventArgs e)
        {
            var tgl = sender as ToggleSwitch;
            if (tgl?.IsChecked == null) return;

            Properties.Settings.Default.MinimizeHostname = (bool) tgl.IsChecked;
            Properties.Settings.Default.Save();

            if ((bool)tgl.IsChecked)
                _receiver.MinimizeHostnames();
            else
                _receiver.UnminimizeHostnames();
        }

        private void HideSmallPackets_OnClickSwitch_Click(object sender, RoutedEventArgs e)
        {
            var tgl = sender as ToggleSwitch;
            if (tgl?.IsChecked == null) return;

            Properties.Settings.Default.HideSmallPackets = (bool)tgl.IsChecked;
            Properties.Settings.Default.Save();

            _receiver.HideSmallPackets = (bool) tgl.IsChecked;
        }
    }
}
