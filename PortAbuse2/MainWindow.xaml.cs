﻿using System;
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
        internal readonly Receiver Receiver;
        private readonly KeyEventsHandling _keyEventsHandling;

        private readonly MainPageViewModel _vm = new MainPageViewModel();

        public MainWindow()
        {
            this.Receiver = new Receiver(this, Properties.Settings.Default.MinimizeHostname,
                Properties.Settings.Default.HideOldConnections,
                Properties.Settings.Default.HideSmallPackets);

            this.InitializeComponent();

            //"hacky" fix of designer glitch
            this.FlyOutGrid.Visibility = Visibility.Visible;

            Admin.CheckAdmin();

            CustomSettings.Load(Properties.Settings.Default.CustomSettings);

            this.LoadInterfaces();

            this.AppListComboBox.ItemsSource = this._allAppWithPorts;

            this.ResultBox.ItemsSource = this.Receiver.ResultObjects;

            Task.Run(this.RefreshLoadProcesses);

            this.SettingPage.SetMainWindow(this);

            this._keyEventsHandling = new KeyEventsHandling();

            this._keyEventsHandling.SignForKeyAction(KeyActionType.BlockAllToggle, this.SettingPage.ToggleBlock);

#if DEBUG
            this.FillDummyData();
#endif
        }

        // ReSharper disable once UnusedMember.Local
        private void FillDummyData()
        {
            this.Receiver.ResultObjects.Add(new ResultObject
            {
                SourceAddress = new IPAddress(new byte[]{100,100,100,100}),
                DestinationAddress = new IPAddress(new byte[] { 100, 100, 100, 100 }),
                Hostname = "Test1",
                PackagesReceived = 662
            });

            this.Receiver.ResultObjects.Add(new ResultObject
            {
                SourceAddress = new IPAddress(new byte[] { 101, 100, 100, 100 }),
                DestinationAddress = new IPAddress(new byte[] { 101, 100, 100, 100 }),
                Hostname = "Test1",
                PackagesReceived = 32567
            });
            this.Receiver.ResultObjects.Add(new ResultObject
            {
                SourceAddress = new IPAddress(new byte[] { 102, 100, 100, 100 }),
                DestinationAddress = new IPAddress(new byte[] { 102, 100, 100, 100 }),
                Hostname = "Test1",
                PackagesReceived = 1000000000
            });
            this.Receiver.ResultObjects.Add(new ResultObject
            {
                SourceAddress = new IPAddress(new byte[] { 103, 100, 100, 100 }),
                DestinationAddress = new IPAddress(new byte[] { 103, 100, 100, 100 }),
                Hostname = "Test1",
                PackagesReceived = 3756412
            });
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

        public void RemapBlockButtons(int amount)
        {
            BlockTimeContainer.CurrentBlockTime = amount;
        }

        private void LoadInterfaces()
        {
            var interfaces = IpInterface.GetIpInterfaces();
            var itemsSource = interfaces as string[] ?? interfaces.ToArray();
            this.InterfaceBox.ItemsSource = itemsSource;
            if (itemsSource.Length == 1)
            {
                this.InterfaceBox.SelectedIndex = 0;
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
                if (this.Receiver.SelectedAppEntry == null)
                {
                    var wfApp = this._allAppWithPorts.FirstOrDefault(x => x.Name.Contains("warframe", StringComparison.OrdinalIgnoreCase));
                    if (wfApp != null)
                    {
                        this.AppListComboBox.SelectedItem = wfApp;
                    }
                }

                if (!(sender is Button btn) || this.Receiver.SelectedAppEntry == null) return;
                if (!this.Receiver.ContinueCapturing)
                {
                    this.Receiver.Clear();
                    btn.Content = "Stop";
                    if (this.InterfaceBox.SelectedItem == null)
                    {
                        if (CustomSettings.Instance.PreviousInterface.Empty())
                            this.InterfaceBox.SelectedIndex = 0;
                        else
                            this.InterfaceBox.SelectedItem = CustomSettings.Instance.PreviousInterface;
                    }

                    this.Receiver.StartListener(this.InterfaceBox.SelectedItem?.ToString());
                    CustomSettings.Instance.PreviousInterface = this.InterfaceBox.SelectedItem?.ToString();
                    var red = this.FindResource("PaLightRed") as SolidColorBrush;
                    btn.Background = red;
                    this._vm.IsRunning = true;
                }
                else
                {
                    btn.Content = "Start";
                    this.Receiver.Stop();
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
            this.Receiver.Stop();
            this._keyEventsHandling.Stop();
            Block.ShutAll = true;
            await Block.Wait();
            Properties.Settings.Default.Save();
        }

        private void AppListComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cb = sender as ComboBox;
            if (!(cb?.SelectedItem is AppIconEntry app)) return;
            this.Receiver.SelectedAppEntry = app;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.ExtraOptions.IsOpen = !this.ExtraOptions.IsOpen;
        }
    }
}
