using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using PortAbuse2.Properties;
using PortAbuse2.ViewModels;
using TiqUtils.TypeSpecific;
using Admin = PortAbuse2.Common.Admin;

namespace PortAbuse2;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    private readonly ObservableCollection<AppIconEntry> _allAppWithPorts = new();
    private readonly KeyEventsHandling _keyEventsHandling;

    private readonly MainLogicViewModel _vm;

    public MainWindow()
    {
        _vm = new MainLogicViewModel(Dispatcher,
            Settings.Default.MinimizeHostname,
            Settings.Default.HideOldConnections,
            Settings.Default.HideSmallPackets);

        DataContext = _vm;

        InitializeComponent();

        //"hacky" fix of designer glitch
        FlyOutGrid.Visibility = Visibility.Visible;

        Admin.CheckAdmin();

        CustomSettings.Load(Settings.Default.CustomSettings);

        Loaded += PostLoad;

        LoadInterfaces();

        AppListComboBox.ItemsSource = _allAppWithPorts;

        Task.Run(RefreshLoadProcesses);

        SettingPage.SetViewModel(_vm);

        _keyEventsHandling = new KeyEventsHandling();

        _keyEventsHandling.SignForKeyAction(KeyActionType.BlockAllToggle, SettingPage.ToggleBlock);

#if DEBUG
        FillDummyData();
#endif
    }

    private void PostLoad(object sender, RoutedEventArgs e)
    {
        Task.Run(Validate);
    }

    private async Task Validate()
    {
        var validationResult = _vm.Validate();
        if (validationResult.Result != ResultType.NoIssues && validationResult.Messages != null &&
            validationResult.Messages.Any())
        {
            var dialogSettings = new MetroDialogSettings
            {
                AffirmativeButtonText = "Download",
                NegativeButtonText = "Whatever",
                AnimateShow = true,
                AnimateHide = false
            };
            await Dispatcher.InvokeAsync(async () =>
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
        _vm.Add(new ConnectionInformation
        {
            SourceAddress = new IPAddress(new byte[] { 100, 100, 100, 100 }),
            DestinationAddress = new IPAddress(new byte[] { 100, 100, 100, 100 }),
            Hostname = "Test1",
            PackagesReceived = 662
        });

        _vm.Add(new ConnectionInformation
        {
            SourceAddress = new IPAddress(new byte[] { 101, 100, 100, 100 }),
            DestinationAddress = new IPAddress(new byte[] { 101, 100, 100, 100 }),
            Hostname = "Test1",
            PackagesReceived = 32567
        });
        _vm.Add(new ConnectionInformation
        {
            SourceAddress = new IPAddress(new byte[] { 102, 100, 100, 100 }),
            DestinationAddress = new IPAddress(new byte[] { 102, 100, 100, 100 }),
            Hostname = "Test1",
            PackagesReceived = 1000000000
        });
        _vm.Add(new ConnectionInformation
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

        _vm.Add(googleDns);

        GeoWorker.InsertGeoDataQueue(googleDns);
    }

    private async Task RefreshLoadProcesses()
    {
        var apps = await AppList.GetRunningApplications();

        foreach (var p in apps)
        {
            if (_allAppWithPorts.All(x => x.InstancePid != p.InstancePid))
            {
                var dispatcher = Dispatcher;
                if (dispatcher != null)
                {
                    await dispatcher.BeginInvoke(new ThreadStart(delegate
                    {
                        p.Icon = AppIcon.GetIcon(p.FullName, false)!;
                        _allAppWithPorts.Add(p);
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

            _vm.Interfaces = new ObservableCollection<IpInterface>(interfaces);
        }
        catch
        {
            _vm.Interfaces = new ObservableCollection<IpInterface>();
        }
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        _allAppWithPorts.Clear();
        Task.Run(RefreshLoadProcesses);
    }

    private void SwitchButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_vm.Receiver.SelectedAppEntry == null)
            {
                var wfApp = _allAppWithPorts.FirstOrDefault(x =>
                    x.Name.Contains("warframe", StringComparison.OrdinalIgnoreCase));
                if (wfApp != null)
                {
                    AppListComboBox.SelectedItem = wfApp;
                }
            }

            if (!(sender is Button btn) || _vm.Receiver.SelectedAppEntry == null)
            {
                return;
            }

            if (!_vm.Receiver.ContinueCapturing)
            {
                _vm.Receiver.Clear();
                btn.Content = "Stop";
                if (_vm.SelectedInterface == null)
                {
                    if (CustomSettings.Instance.PreviousInterface.Empty())
                    {
                        _vm.SelectedInterface = _vm.Interfaces!.FirstOrDefault();
                    }
                    else
                    {
                        _vm.SelectedInterface =
                            _vm.Interfaces!.FirstOrDefault(x => x.HwName == CustomSettings.Instance.PreviousInterface);
                    }
                }

                _vm.Receiver.StartListener(_vm.SelectedInterface);
                CustomSettings.Instance.PreviousInterface = _vm.SelectedInterface?.HwName;
                var red = FindResource("PaLightRed") as SolidColorBrush;
                btn.Background = red;
                _vm.IsRunning = true;
            }
            else
            {
                btn.Content = "Start";
                _vm.Receiver.Stop();
                var green = FindResource("PaLightGreen") as SolidColorBrush;
                btn.Background = green;
                _vm.IsRunning = false;
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

        var curEx = ex;

        while (curEx != null)
        {
            sb.AppendLine("------------------------");
            sb.AppendLine($"Message: {curEx.Message}");
            sb.AppendLine($"StackTrace: {curEx.StackTrace}");
            sb.AppendLine("------------------------");

            curEx = ex.InnerException;
        }

        return sb.ToString();
    }

    private async void MetroWindow_Closing(object sender, CancelEventArgs e)
    {
        Settings.Default.CustomSettings = CustomSettings.SaveToString();
        _vm.Receiver.Stop();
        _keyEventsHandling.Stop();
        Block.ShutAll = true;
        await Block.Wait();
        Settings.Default.Save();
    }

    private void AppListComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var cb = sender as ComboBox;
        if (!(cb?.SelectedItem is AppIconEntry app))
        {
            return;
        }

        _vm.Receiver.SelectedAppEntry = app;
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        ExtraOptions.IsOpen = !ExtraOptions.IsOpen;
    }
}