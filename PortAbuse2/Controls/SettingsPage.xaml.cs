using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ControlzEx.Theming;
using MahApps.Metro.Controls;
using PortAbuse2.Annotations;
using PortAbuse2.Core.Geo;
using PortAbuse2.Core.WindowsFirewall;
using PortAbuse2.Properties;
using PortAbuse2.ViewModels;
using TiqUtils.Events.Controls;
using TiqUtils.Wpf.Helpers;

namespace PortAbuse2.Controls;

/// <summary>
///     Interaction logic for SettingsPage.xaml
/// </summary>
public partial class SettingsPage : INotifyPropertyChanged
{
    private MainLogicViewModel? _vm;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public SettingsPage()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        InitializeComponent();
        GeoProviderBox.ItemsSource = GeoWorker.GeoProviders;
    }

    public BlockMode SelectedBlockSate => Block.DefaultBlockMode;

    public bool BlockNew
    {
        get => _vm?.Receiver.BlockNew ?? false;
        set
        {
            _vm!.Receiver.BlockNew = value;
            OnPropertyChanged();
            UpdateThemeForBlock();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public void SetViewModel(MainLogicViewModel vm)
    {
        _vm = vm;
        LoadSettings();
    }

    private void LoadSettings()
    {
        MinimizeHostnames.IsOn = Settings.Default.MinimizeHostname;
        HideOldRecords.IsOn = Settings.Default.HideOldConnections;
        HideSmallPackets.IsOn = Settings.Default.HideSmallPackets;
        SecondsBlockBox.Text = Settings.Default.BlockSeconds.ToString();

        GeoProviderBox.SelectedItem = GeoWorker.SelectProviderByName(Settings.Default.GeoProvider);
        BlockDirectionBox.SelectedValue = (BlockMode)Settings.Default.BlockType;
        Block.DefaultBlockMode = (BlockMode)Settings.Default.BlockType;

        BlockTimeContainer.CurrentBlockTime = Settings.Default.BlockSeconds;

        VersionNumberBlock.Text = $"Lunatiq© - v{Assembly.GetExecutingAssembly().GetName().Version}";
    }

    public void ToggleBlock()
    {
        BlockNew = !BlockNew;
        UpdateThemeForBlock();
    }

    private void UpdateThemeForBlock()
    {
        ThemeManager.Current.ChangeTheme(Application.Current,
            (BlockNew
                ? ThemeManager.Current.GetTheme("Light.Red")
                : ThemeManager.Current.GetTheme("Light.Steel"))!);
    }

    private void HideOldRecords_OnClickSwitch_Click(object sender, RoutedEventArgs e)
    {
        var tgl = sender as ToggleSwitch;
        if (tgl?.IsOn == null)
        {
            return;
        }

        Settings.Default.HideOldConnections = tgl.IsOn;
        Settings.Default.Save();

        if (tgl.IsOn)
        {
            _vm!.Receiver.HideOld();
        }
        else
        {
            _vm!.Receiver.ShowOld();
        }
    }

    private void MinimizeHostnames_OnClickSwitch_Click(object sender, RoutedEventArgs e)
    {
        var tgl = sender as ToggleSwitch;
        if (tgl?.IsOn == null)
        {
            return;
        }

        Settings.Default.MinimizeHostname = tgl.IsOn;
        Settings.Default.Save();

        if (tgl.IsOn)
        {
            _vm!.Receiver.MinimizeHostnames();
        }
        else
        {
            _vm!.Receiver.MaximizeHostnames();
        }
    }

    private void HideSmallPackets_OnClickSwitch_Click(object sender, RoutedEventArgs e)
    {
        var tgl = sender as ToggleSwitch;
        if (tgl?.IsOn == null)
        {
            return;
        }

        Settings.Default.HideSmallPackets = tgl.IsOn;
        Settings.Default.Save();

        _vm!.Receiver.HideSmallPackets = tgl.IsOn;
    }

    private void ShowAllHiddenIps_OnClick(object sender, RoutedEventArgs e)
    {
        var tgl = sender as ToggleSwitch;
        if (tgl?.IsOn == null)
        {
            return;
        }

        if (tgl.IsOn)
        {
            _vm!.Receiver.SetForceShowHiddenIps();
        }
        else
        {
            _vm!.Receiver.SetForceShowHiddenIps(false);
        }
    }

    private void SecondsBlockBox_OnKeyDown(object sender, KeyEventArgs e)
    {
        KeyConverter kc = new();
        var ch = kc.ConvertToString(e.Key);
        e.Handled = ControlsInput.OnlyNum(ch![0]);
    }

    private void SecondsBlockBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var tb = sender as TextBox;
        if (string.IsNullOrEmpty(tb?.Text))
        {
            return;
        }

        if (!int.TryParse(tb.Text, out var amount))
        {
            return;
        }

        Settings.Default.BlockSeconds = amount;
        Settings.Default.Save();
        _vm!.BlockAmount = amount;
    }

    private void GeoProviderBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var cb = sender as ComboBox;
        if (!(cb?.SelectedItem is IGeoService item))
        {
            return;
        }

        GeoWorker.SelectProviderByObject(item);
        foreach (var ro in _vm!.DetectedConnections)
        {
            ro?.Geo.Reset();
            GeoWorker.InsertGeoDataQueue(ro);
        }

        Settings.Default.GeoProvider = item.Name;
        Settings.Default.Save();
    }

    private void BlockDirectionBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var cb = sender as ComboBox;
        if (!(cb?.SelectedItem is ValueDescription item))
        {
            return;
        }

        Settings.Default.BlockType = Convert.ToInt32(item.Value);
        Block.DefaultBlockMode = (BlockMode)item.Value;
        Settings.Default.Save();
    }

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}