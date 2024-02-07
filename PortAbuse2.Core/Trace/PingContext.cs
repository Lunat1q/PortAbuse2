using System.Collections.ObjectModel;
using System.Net;
using PortAbuse2.Core.Common;

namespace PortAbuse2.Core.Trace
{
    public class PingContext : PaNotified
    {
        private ObservableCollection<PingEntry> _items = new ObservableCollection<PingEntry>();
        private IPAddress _target;
        private bool _isComplete;

        public ObservableCollection<PingEntry> Items
        {
            get => this._items;
            set
            {
                this._items = value;
                this.OnPropertyChanged();
            }
        }

        public IPAddress Target
        {
            get => this._target;
            set
            {
                this._target = value;
                this.OnPropertyChanged();
            }
        }

        public bool IsRunning { get; set; }
    }
}