using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MahApps.Metro.Controls.Dialogs;
using PortAbuse2.Applications;
using PortAbuse2.Core.Common;
using PortAbuse2.Core.Geo;
using PortAbuse2.Core.Ip;
using PortAbuse2.Core.Result;
using PortAbuse2.Core.WindowsFirewall;
using PortAbuse2.KeyCapture;
using PortAbuse2.ViewModels;
using TiqUtils.TypeSpecific;
using Admin = PortAbuse2.Common.Admin;

namespace PortAbuse2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly ObservableCollection<AppIconEntry> _allAppWithPorts = new ObservableCollection<AppIconEntry>();
        private readonly KeyEventsHandling _keyEventsHandling;

        private readonly MainLogicViewModel _vm ;

        public MainWindow()
        {
            this._vm = new MainLogicViewModel();

            this._vm.InitNewReceiver(
                this.Dispatcher, 
                Properties.Settings.Default.MinimizeHostname,
                Properties.Settings.Default.HideOldConnections,
                Properties.Settings.Default.HideSmallPackets
            );

            this.DataContext = this._vm;

            this.InitializeComponent();

            //"hacky" fix of designer glitch
            this.FlyOutGrid.Visibility = Visibility.Visible;

            Admin.CheckAdmin();

            CustomSettings.Load(Properties.Settings.Default.CustomSettings);

            this.Loaded += new RoutedEventHandler(this.PostLoad);

            this.LoadInterfaces();

            this.AppListComboBox.ItemsSource = this._allAppWithPorts;

            Task.Run(this.RefreshLoadProcesses);

            this.SettingPage.SetViewModel(this._vm);

            this._keyEventsHandling = new KeyEventsHandling();

            this._keyEventsHandling.SignForKeyAction(KeyActionType.BlockAllToggle, this.SettingPage.ToggleBlock);

#if DEBUG
            this.FillDummyData();
#endif
        }

        private void PostLoad(object sender, RoutedEventArgs e)
        {
            Task.Run(this.Validate);
        }

        private async Task Validate()
        {
            var validationResult = this._vm.Validate();
            if (validationResult.Result != ResultType.NoIssues && validationResult.Messages != null && validationResult.Messages.Any())
            {
                var dialogSettings = new MetroDialogSettings
                {
                    AffirmativeButtonText = "Download",
                    NegativeButtonText = "Whatever",
                    AnimateShow = true,
                    AnimateHide = false
                };
                await this.Dispatcher.InvokeAsync(async () =>
                {
                    var result = await this.ShowMessageAsync(
                        $"Validation failed with {validationResult.Result} message",
                        string.Join("\r\n", validationResult.Messages),
                        MessageDialogStyle.AffirmativeAndNegative,
                        dialogSettings);
                    if (result == MessageDialogResult.Affirmative)
                    {
                        Process.Start("https://nmap.org/npcap/");
                    }
                });
            }
        }

        // ReSharper disable once UnusedMember.Local
        private void FillDummyData()
        {
            this._vm.Add(new ConnectionInformation
            {
                SourceAddress = new IPAddress(new byte[]{100,100,100,100}),
                DestinationAddress = new IPAddress(new byte[] { 100, 100, 100, 100 }),
                Hostname = "Test1",
                PackagesReceived = 662
            });

            this._vm.Add(new ConnectionInformation
            {
                SourceAddress = new IPAddress(new byte[] { 101, 100, 100, 100 }),
                DestinationAddress = new IPAddress(new byte[] { 101, 100, 100, 100 }),
                Hostname = "Test1",
                PackagesReceived = 32567
            });
            this._vm.Add(new ConnectionInformation
            {
                SourceAddress = new IPAddress(new byte[] { 102, 100, 100, 100 }),
                DestinationAddress = new IPAddress(new byte[] { 102, 100, 100, 100 }),
                Hostname = "Test1",
                PackagesReceived = 1000000000
            });
            this._vm.Add(new ConnectionInformation
            {
                SourceAddress = new IPAddress(new byte[] { 103, 100, 100, 100 }),
                DestinationAddress = new IPAddress(new byte[] { 103, 100, 100, 100 }),
                Hostname = "Test1",
                PackagesReceived = 3756412
            });

            var googleDns = new ConnectionInformation
            {
                SourceAddress = new IPAddress(new byte[] { 8, 8, 8, 8 }),
                DestinationAddress = new IPAddress(new byte[] { 8, 8, 8, 8 }),
                Hostname = "Test Google DNS",
                PackagesReceived = 888
            };

            this._vm.Add(googleDns);

            GeoWorker.InsertGeoDataQueue(googleDns);
        }

        private async Task RefreshLoadProcesses()
        {
            var apps = await AppList.GetRunningApplications();
            if (apps == null) return;
            foreach (var p in apps)
            {
                if (p == null) continue;
                if (this._allAppWithPorts.All(x => x.InstancePid != p.InstancePid))
                {
                    var dispatcher = this.Dispatcher;
                    if (dispatcher != null)
                    {
                        await dispatcher.BeginInvoke(new ThreadStart(delegate
                        {
                            p.Icon = AppIcon.GetIcon(p.FullName, false);
                            this._allAppWithPorts.Add(p);
                        }));
                    }
                }
            }
        }

        private void LoadInterfaces()
        {
            try
            {
                var interfaces = IpInterface.GetIpInterfaces();

                this._vm.Interfaces = new ObservableCollection<IpInterface>(interfaces);
            }
            catch
            {
                this._vm.Interfaces = new ObservableCollection<IpInterface>();
            }
        }
        
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            this._allAppWithPorts.Clear();
            Task.Run(this.RefreshLoadProcesses);
        }

        private void SwitchButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this._vm.Receiver.SelectedAppEntry == null)
                {
                    var wfApp = this._allAppWithPorts.FirstOrDefault(x => x.Name.Contains("warframe", StringComparison.OrdinalIgnoreCase));
                    if (wfApp != null)
                    {
                        this.AppListComboBox.SelectedItem = wfApp;
                    }
                }

                if (!(sender is Button btn) || this._vm.Receiver.SelectedAppEntry == null) return;
                if (!this._vm.Receiver.ContinueCapturing)
                {
                    this._vm.Receiver.Clear();
                    btn.Content = "Stop";
                    if (this._vm.SelectedInterface == null)
                    {
                        if (CustomSettings.Instance.PreviousInterface.Empty())
                            this._vm.SelectedInterface = this._vm.Interfaces.FirstOrDefault();
                        else
                            this._vm.SelectedInterface = this._vm.Interfaces.FirstOrDefault(x=>x.HwName == CustomSettings.Instance.PreviousInterface);
                    }

                    this._vm.Receiver.StartListener(this._vm.SelectedInterface);
                    CustomSettings.Instance.PreviousInterface = this._vm.SelectedInterface?.HwName;
                    var red = this.FindResource("PaLightRed") as SolidColorBrush;
                    btn.Background = red;
                    this._vm.IsRunning = true;
                }
                else
                {
                    btn.Content = "Start";
                    this._vm.Receiver.Stop();
                    var green = this.FindResource("PaLightGreen") as SolidColorBrush;
                    btn.Background = green;
                    this._vm.IsRunning = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Start failed, reason:\r\n{GetTextFromException(ex)}");
            }
        }

        private static string GetTextFromException(Exception ex)
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
            this._vm.Receiver.Stop();
            this._keyEventsHandling.Stop();
            Block.ShutAll = true;
            await Block.Wait();
            Properties.Settings.Default.Save();
        }

        private void AppListComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cb = sender as ComboBox;
            if (!(cb?.SelectedItem is AppIconEntry app)) return;
            this._vm.Receiver.SelectedAppEntry = app;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.ExtraOptions.IsOpen = !this.ExtraOptions.IsOpen;
        }
    }
}
