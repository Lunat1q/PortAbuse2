using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using PortAbuse2.Annotations;
using PortAbuse2.Controls;
using PortAbuse2.Core.Common;
using PortAbuse2.Core.Ip;
using PortAbuse2.Core.Listener;
using PortAbuse2.Core.Result;
using PortAbuse2.Listener;

namespace PortAbuse2.ViewModels;

public class MainLogicViewModel : INotifyPropertyChanged, IResultReceiver
{
    private readonly object _detectionLock = new();
    private ObservableCollection<ConnectionInformation?> _detectedConnections = new();
    private ObservableCollection<IpInterface>? _interfaces;
    private bool _isRunning;
    private IpInterface? _selectedInterface;

    public MainLogicViewModel(Dispatcher dispatcher,
                              bool minimizeHostname = false,
                              bool hideOld = false,
                              bool hideSmall = false
    )
    {
        Receiver = new Receiver(this, dispatcher, minimizeHostname, hideOld, hideSmall);
    }

    public bool IsRunning
    {
        get => _isRunning;
        set
        {
            if (value == _isRunning)
            {
                return;
            }

            _isRunning = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<IpInterface>? Interfaces
    {
        get => _interfaces;
        set
        {
            if (Equals(value, _interfaces))
            {
                return;
            }

            _interfaces = value;
            OnPropertyChanged();
        }
    }

    public IpInterface? SelectedInterface
    {
        get => _selectedInterface;
        set
        {
            if (Equals(value, _selectedInterface))
            {
                return;
            }

            _selectedInterface = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<ConnectionInformation?> DetectedConnections
    {
        get => _detectedConnections;
        set
        {
            if (Equals(value, _detectedConnections))
            {
                return;
            }

            _detectedConnections = value;
            OnPropertyChanged();
        }
    }

    public int BlockAmount
    {
        get => BlockTimeContainer.CurrentBlockTime;
        set
        {
            if (value == BlockTimeContainer.CurrentBlockTime)
            {
                return;
            }

            BlockTimeContainer.CurrentBlockTime = value;
            OnPropertyChanged();
        }
    }

    public CoreReceiver Receiver { get; }
    public event PropertyChangedEventHandler? PropertyChanged;

    public void Reset()
    {
        DetectedConnections.Clear();
    }

    public void Add(ConnectionInformation? result)
    {
        DetectedConnections.Add(result);
    }

    public void AddAsync(ConnectionInformation result)
    {
        lock (_detectionLock)
        {
            DetectedConnections.Add(result);
        }
    }

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }


    public void InitNewReceiver(Dispatcher dispatcher,
                                bool minimizeHostname = false,
                                bool hideOld = false,
                                bool hideSmall = false)
    {
    }

    public ValidationResult Validate()
    {
        var validator = new InstallationValidator();
        return validator.Validate();
    }
}