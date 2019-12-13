using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MahApps.Metro;
using MahApps.Metro.Controls;
using PortAbuse2.Annotations;
using PortAbuse2.Core.Geo;
using PortAbuse2.Core.WindowsFirewall;
using PortAbuse2.ViewModels;
using TiqUtils.Conversion;
using TiqUtils.Events.Controls;
using TiqUtils.Wpf.Helpers;

namespace PortAbuse2.Controls
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : INotifyPropertyChanged
    {
        private MainLogicViewModel _vm;
        public BlockMode SelectedBlockSate => Block.DefaultBlockMode;

        public bool? BlockNew
        {
            get => this._vm?.Receiver.BlockNew;
            set
            {
                if (this._vm != null && value != null)
                {
                    this._vm.Receiver.BlockNew = (bool)value;
                    this.OnPropertyChanged();
                }
            }
        }

        public SettingsPage()
        {
            this.InitializeComponent();
            this.GeoProviderBox.ItemsSource = GeoWorker.GeoProviders;
        }

        public void SetViewModel(MainLogicViewModel vm)
        {
            this._vm = vm;
            this.LoadSettings();
        }

        private void LoadSettings()
        {
            this.MinimizeHostnames.IsChecked = Properties.Settings.Default.MinimizeHostname;
            this.HideOldRecords.IsChecked = Properties.Settings.Default.HideOldConnections;
            this.HideSmallPackets.IsChecked = Properties.Settings.Default.HideSmallPackets;
            this.SecondsBlockBox.Text = Properties.Settings.Default.BlockSeconds.ToString();

            this.GeoProviderBox.SelectedItem = GeoWorker.SelectProviderByName(Properties.Settings.Default.GeoProvider);
            this.BlockDirectionBox.SelectedValue = (BlockMode)Properties.Settings.Default.BlockType;
            Block.DefaultBlockMode = (BlockMode)Properties.Settings.Default.BlockType;

            BlockTimeContainer.CurrentBlockTime = Properties.Settings.Default.BlockSeconds;

            this.VersionNumberBlock.Text = $"Lunatiq© - v{Assembly.GetExecutingAssembly().GetName().Version}";
        }

        public void ToggleBlock()
        {
            this.BlockNew = !this.BlockNew;
            var accent = ThemeManager.GetAccent((bool) this.BlockNew ? "Red" : "Steel");
            ThemeManager.ChangeAppStyle(Application.Current, accent, ThemeManager.GetAppTheme("BaseLight"));
        }

        private void HideOldRecords_OnClickSwitch_Click(object sender, RoutedEventArgs e)
        {
            var tgl = sender as ToggleSwitch;
            if (tgl?.IsChecked == null) return;

            Properties.Settings.Default.HideOldConnections = (bool)tgl.IsChecked;
            Properties.Settings.Default.Save();

            if ((bool)tgl.IsChecked)
                this._vm.Receiver.HideOld();
            else
                this._vm.Receiver.ShowOld();
        }

        private void MinimizeHostnames_OnClickSwitch_Click(object sender, RoutedEventArgs e)
        {
            var tgl = sender as ToggleSwitch;
            if (tgl?.IsChecked == null) return;

            Properties.Settings.Default.MinimizeHostname = (bool)tgl.IsChecked;
            Properties.Settings.Default.Save();

            if ((bool)tgl.IsChecked)
                this._vm.Receiver.MinimizeHostnames();
            else
                this._vm.Receiver.MaximizeHostnames();
        }

        private void HideSmallPackets_OnClickSwitch_Click(object sender, RoutedEventArgs e)
        {
            var tgl = sender as ToggleSwitch;
            if (tgl?.IsChecked == null) return;

            Properties.Settings.Default.HideSmallPackets = (bool)tgl.IsChecked;
            Properties.Settings.Default.Save();

            this._vm.Receiver.HideSmallPackets = (bool)tgl.IsChecked;
        }

        private void ShowAllHiddenIps_OnClick(object sender, RoutedEventArgs e)
        {
            var tgl = sender as ToggleSwitch;
            if (tgl?.IsChecked == null) return;

            if ((bool)tgl.IsChecked)
                this._vm.Receiver.SetForceShowHiddenIps();
            else
                this._vm.Receiver.SetForceShowHiddenIps(false);
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
            this._vm.BlockAmount = amount;
        }

        private void GeoProviderBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cb = sender as ComboBox;
            if (!(cb?.SelectedItem is IGeoService item)) return;
            GeoWorker.SelectProviderByObject(item);
            foreach (var ro in this._vm.DetectedConnections)
            {
                ro.Geo.Reset();
                GeoWorker.InsertGeoDataQueue(ro);
            }
            Properties.Settings.Default.GeoProvider = item.Name;
            Properties.Settings.Default.Save();
        }

        private void BlockDirectionBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cb = sender as ComboBox;
            if (!(cb?.SelectedItem is ValueDescription item)) return;
            Properties.Settings.Default.BlockType = Convert.ToInt32(item.Value);
            Block.DefaultBlockMode = (BlockMode)item.Value;
            Properties.Settings.Default.Save();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
