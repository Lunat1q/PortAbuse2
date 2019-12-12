using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using PortAbuse2.Annotations;
using PortAbuse2.Core.Ip;

namespace PortAbuse2.ViewModels
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        private bool _isRunning;
        private ObservableCollection<IpInterface> _interfaces;
        private IpInterface _selectedInterface;
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
    }
}
