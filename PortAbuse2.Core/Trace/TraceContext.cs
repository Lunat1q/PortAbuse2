using System.Collections.ObjectModel;
using System.Net;
using PortAbuse2.Core.Common;

namespace PortAbuse2.Core.Trace
{
    public class TraceContext : PaNotified
    {
        private ObservableCollection<TraceEntry?> _items = new ObservableCollection<TraceEntry?>();
        private IPAddress _target;
        private bool _isComplete;

        public ObservableCollection<TraceEntry?> Items
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

        public bool IsComplete
        {
            get => this._isComplete;
            set
            {
                this._isComplete = value; 
                this.OnPropertyChanged();
            }
        }
    }
}