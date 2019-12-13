using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using PortAbuse2.Annotations;
using PortAbuse2.Controls;
using PortAbuse2.Core.Ip;
using PortAbuse2.Core.Listener;
using PortAbuse2.Core.Result;
using PortAbuse2.Listener;

namespace PortAbuse2.ViewModels
{
    public class MainLogicViewModel : INotifyPropertyChanged, IResultReceiver
    {
        private bool _isRunning;
        private readonly object _detectionLock = new object();
        private ObservableCollection<IpInterface> _interfaces;
        private IpInterface _selectedInterface;
        private ObservableCollection<ConnectionInformation> _detectedConnections = new ObservableCollection<ConnectionInformation>();
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool IsRunning
        {
            get => this._isRunning;
            set
            {
                if (value == this._isRunning) return;
                this._isRunning = value;
                this.OnPropertyChanged();
            }
        }

        public ObservableCollection<IpInterface> Interfaces
        {
            get => this._interfaces;
            set
            {
                if (Equals(value, this._interfaces)) return;
                this._interfaces = value;
                this.OnPropertyChanged();
            }
        }

        public IpInterface SelectedInterface
        {
            get => this._selectedInterface;
            set
            {
                if (Equals(value, this._selectedInterface)) return;
                this._selectedInterface = value;
                this.OnPropertyChanged();
            }
        }

        public ObservableCollection<ConnectionInformation> DetectedConnections
        {
            get => this._detectedConnections;
            set
            {
                if (Equals(value, this._detectedConnections)) return;
                this._detectedConnections = value;
                this.OnPropertyChanged();
            }
        }

        public int BlockAmount
        {
            get => BlockTimeContainer.CurrentBlockTime;
            set
            {
                if (value == BlockTimeContainer.CurrentBlockTime) return;
                BlockTimeContainer.CurrentBlockTime = value;
                this.OnPropertyChanged();
            }
        }

        public CoreReceiver Receiver { get; set; }


        public void InitNewReceiver(Dispatcher dispatcher, bool minimizeHostname = false, bool hideOld = false, bool hideSmall = false)
        {
            this.Receiver = new Receiver(this, dispatcher, minimizeHostname, hideOld, hideSmall);
        }

        public void Reset()
        {
            this.DetectedConnections.Clear();
        }

        public void Add(ConnectionInformation result)
        {
            this.DetectedConnections.Add(result);
        }

        public void AddAsync(ConnectionInformation result)
        {
            lock (this._detectionLock)
            {
                this.DetectedConnections.Add(result);
            }
        }
    }
}
