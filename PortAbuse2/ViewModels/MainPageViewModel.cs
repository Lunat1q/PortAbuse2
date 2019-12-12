using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using PortAbuse2.Annotations;

namespace PortAbuse2.ViewModels
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        private bool _isRunning;
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
    }
}
