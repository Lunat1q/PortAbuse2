using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PortAbuse2.Applications;
using PortAbuse2.Controls;
using PortAbuse2.Core.Common;
using PortAbuse2.Core.Ip;
using PortAbuse2.Core.Result;
using PortAbuse2.Core.WindowsFirewall;
using PortAbuse2.KeyCapture;
using PortAbuse2.Listener;
using TiqUtils.TypeSpeccific;
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
        private readonly KeyEventsHandling _keyEventsHandling;

        public MainWindow()
        {
            Receiver = new Receiver(this, Properties.Settings.Default.MinimizeHostname,
                Properties.Settings.Default.HideOldConnections,
                Properties.Settings.Default.HideSmallPackets);

            InitializeComponent();

            //"hacky" fix of designer glitch
            FlyOutGrid.Visibility = Visibility.Visible;

            Admin.CheckAdmin();

            CustomSettings.Load(Properties.Settings.Default.CustomSettings);

            LoadInterfaces();

            AppListComboBox.ItemsSource = _allAppWithPorts;

            ResultBox.ItemsSource = Receiver.ResultObjects;

            Task.Run(RefreshLoadProceses);

            SettingPage.SetMainWindow(this);

            _keyEventsHandling = new KeyEventsHandling();

            _keyEventsHandling.SignForKeyAction(KeyActionType.BlockAllToggle, SettingPage.ToggleBlock);

#if DEBUG
            FillDummyData();
#endif
        }

        // ReSharper disable once UnusedMember.Local
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
            if (apps == null) return;
            foreach (var p in apps)
            {
                if (p == null) continue;
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
            try
            {
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
                    btn.Content = "Stop";
                    if (InterfaceBox.SelectedItem == null)
                    {
                        if (CustomSettings.Instance.PreviousInterface.Empty())
                            InterfaceBox.SelectedIndex = 0;
                        else
                            InterfaceBox.SelectedItem = CustomSettings.Instance.PreviousInterface;
                    }
                    Receiver.StartListener(InterfaceBox.SelectedItem?.ToString());
                    CustomSettings.Instance.PreviousInterface = InterfaceBox.SelectedItem?.ToString();
                    var red = FindResource("PaLightRed") as SolidColorBrush;
                    btn.Background = red;
                }
                else
                {
                    btn.Content = "Start";
                    Receiver.Stop();
                    var green = FindResource("PaLightGreen") as SolidColorBrush;
                    btn.Background = green;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Start failed, reason:\r\n{GetTextFromException(ex)}");
            }
        }

        private string GetTextFromException(Exception ex)
        {
            var sb = new StringBuilder();

            while (ex != null)
            {
                sb.AppendLine("------------------------");
                sb.AppendLine($"Message: {ex.Message}");
                sb.AppendLine($"StackTrace: {ex.StackTrace}");
                sb.AppendLine("------------------------");

                ex = ex.InnerException;
            }

            return sb.ToString();
        }
       
        
        private async void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.CustomSettings = CustomSettings.SaveToString();
            Receiver.Stop();
            _keyEventsHandling.Stop();
            Block.ShutAll = true;
            await Block.Wait();
            Properties.Settings.Default.Save();
        }

        private void AppListComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cb = sender as ComboBox;
            var app = cb?.SelectedItem as AppIconEntry;
            if (app == null) return;
            Receiver.SelectedAppEntry = app;
        }



        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ExtraOptions.IsOpen = !ExtraOptions.IsOpen;
        }
    }
}
