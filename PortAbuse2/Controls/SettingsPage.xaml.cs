using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MahApps.Metro.Controls;
using PortAbuse2.Core.Geo;
using TiqUtils.Conversion;
using TiqUtils.Events.Controls;

namespace PortAbuse2.Controls
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : UserControl
    {
        private MainWindow _main;

        public SettingsPage()
        {
            InitializeComponent();
            GeoProviderBox.ItemsSource = GeoWorker.GeoProviders;
        }

        public void SetMainWindow(MainWindow main)
        {
            _main = main;
            LoadSettings();
        }

        private void LoadSettings()
        {
            MinimizeHostnames.IsChecked = Properties.Settings.Default.MinimizeHostname;
            HideOldRecords.IsChecked = Properties.Settings.Default.HideOldConnections;
            HideSmallPackets.IsChecked = Properties.Settings.Default.HideSmallPackets;
            SecondsBlockBox.Text = Properties.Settings.Default.BlockSeconds.ToString();
            
            GeoProviderBox.SelectedItem = GeoWorker.SelectProviderByName(Properties.Settings.Default.GeoProvider);
            

            BlockTimeContainer.CurrentBlockTime = Properties.Settings.Default.BlockSeconds;

            VersionNumberBlock.Text = $"Lunatiq© - v{Assembly.GetExecutingAssembly().GetName().Version}";
        }

        private void BlockNewSwitch_Click(object sender, RoutedEventArgs e)
        {
            var tgl = sender as ToggleSwitch;
            if (tgl?.IsChecked != null)
                _main.Receiver.BlockNew = (bool)tgl.IsChecked;
        }

        private void HideOldRecords_OnClickSwitch_Click(object sender, RoutedEventArgs e)
        {
            var tgl = sender as ToggleSwitch;
            if (tgl?.IsChecked == null) return;

            Properties.Settings.Default.HideOldConnections = (bool)tgl.IsChecked;
            Properties.Settings.Default.Save();

            if ((bool)tgl.IsChecked)
                _main.Receiver.HideOld();
            else
                _main.Receiver.ShowOld();
        }

        private void MinimizeHostnames_OnClickSwitch_Click(object sender, RoutedEventArgs e)
        {
            var tgl = sender as ToggleSwitch;
            if (tgl?.IsChecked == null) return;

            Properties.Settings.Default.MinimizeHostname = (bool)tgl.IsChecked;
            Properties.Settings.Default.Save();

            if ((bool)tgl.IsChecked)
                _main.Receiver.MinimizeHostnames();
            else
                _main.Receiver.UnminimizeHostnames();
        }

        private void HideSmallPackets_OnClickSwitch_Click(object sender, RoutedEventArgs e)
        {
            var tgl = sender as ToggleSwitch;
            if (tgl?.IsChecked == null) return;

            Properties.Settings.Default.HideSmallPackets = (bool)tgl.IsChecked;
            Properties.Settings.Default.Save();

            _main.Receiver.HideSmallPackets = (bool)tgl.IsChecked;
        }

        private void ShowAllHiddenIps_OnClick(object sender, RoutedEventArgs e)
        {
            var tgl = sender as ToggleSwitch;
            if (tgl?.IsChecked == null) return;

            if ((bool)tgl.IsChecked)
                _main.Receiver.SetForceShowHiddenIps();
            else
                _main.Receiver.SetForceShowHiddenIps(false);
        }

        private void SecondsBlockBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            var ch = e.Key.GetCharFromKey();
            e.Handled = ControlsInput.OnlyNum(ch);
        }

        private void SecondsBlockBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = sender as TextBox;
            if (string.IsNullOrEmpty(tb?.Text)) return;
            if (!int.TryParse(tb.Text, out int amount)) return;
            Properties.Settings.Default.BlockSeconds = amount;
            Properties.Settings.Default.Save();
            _main.RemapBlockButtons(amount);
        }

        private void GeoProviderBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cb = sender as ComboBox;
            var item = cb?.SelectedItem as IGeoService;
            if (item == null) return;
            GeoWorker.SelectProviderByObject(item);
            foreach (var ro in _main.Receiver.ResultObjects)
            {
                ro.Geo.Reset();
                GeoWorker.InsertGeoDataQueue(ro);
            }
            Properties.Settings.Default.GeoProvider = item.Name;
            Properties.Settings.Default.Save();
        }
    }
}
