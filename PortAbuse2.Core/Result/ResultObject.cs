using System;
using System.Net;
using PortAbuse2.Core.Common;

namespace PortAbuse2.Core.Result
{
    public class ResultObject : PaNotified
    {
        public IPAddress SourceAddress;
        public IPAddress DestinationAddress;
        private bool _blocked;
        private int _packagesReceived;
        private string _extraInfo;
        private bool _haveExtraInfo;
        private bool _hidden;
        private bool _old;
        private long _dataTransfered;
        private GeoData _geo;
        private bool _from;

        public string SourceIp => this.SourceAddress.ToString();
        public string DestIp => this.DestinationAddress.ToString();

        public bool Resolved { get; set; } = false;


        public bool ForceShow
        {
            get => this._forceShow;
            set
            {
                this._forceShow = value;
                this.OnPropertyChanged();
                this.OnPropertyChangedByName(nameof(this.Hidden));
                this.OnPropertyChangedByName(nameof(this.ForseShown));
                this.OnPropertyChangedByName(nameof(this.HiddenProp));
            }
        }

        public bool ForseShown => this._forceShow && this._hidden;

        public int PackagesReceived
        {
            get => this._packagesReceived;
            set
            {
                this._packagesReceived = value;
                this.LastReceivedTime = DateTime.UtcNow.ToUnixTime();
                this.OnPropertyChanged();
            }
        }

        public long DataTransfered
        {
            get => this._dataTransfered;
            set
            {
                this._dataTransfered = value;
                this.OnPropertyChanged();
            }
        }

        public bool Old
        {
            get => this._old;
            set
            {
                this._old = value;
                this.OnPropertyChanged();
            }
        }

        public bool HiddenProp
        {
            get => this._hidden && !this._forceShow;
            set
            {
                this._hidden = value;
                this.OnPropertyChanged();
                this.OnPropertyChangedByName(nameof(this.Hidden));
                this.OnPropertyChangedByName(nameof(this.ForseShown));
            }
        }

        public bool Hidden
        {
            get => this._hidden;
            set
            {
                this._hidden = value;
                this.OnPropertyChanged();
                this.OnPropertyChangedByName(nameof(this.HiddenProp));
                this.OnPropertyChangedByName(nameof(this.ForseShown));
            }
        }

        public long LastReceivedTime { get; set; }

        public long DetectionStamp { get; set; }

        public string ShowIp => this.From ? this.DestIp : this.SourceIp;

        public string ExtraInfo
        {
            get => this._extraInfo;
            set
            {
                this.HaveExtraInfo = !string.IsNullOrEmpty(value);
                this._extraInfo = value;
                this.OnPropertyChanged();
            }
        }

        public bool HaveExtraInfo
        {
            get => this._haveExtraInfo;
            set
            {
                this._haveExtraInfo = value;
                this.OnPropertyChanged(); 
                
            }
        }

        private string _hostname;
        private bool _forceShow;
        public string DetectedHostname { get; set; }

        public string Hostname
        {
            get => this._hostname;
            set
            {
                this._hostname = value;
                this.OnPropertyChanged();
            }
        }

        public AppEntry Application { get; set; }

        public GeoData Geo
        {
            get => this._geo ?? (this._geo = new GeoData());
            set
            {
                this._geo = value;
                this.OnPropertyChanged();
            }
        }

      

        public bool From
        {
            get => this._from;
            set
            {
                this._from = value;
                this.OnPropertyChanged();
            }
        }

        public bool Blocked
        {
            get => this._blocked;
            set
            {
                this._blocked = value;
                this.OnPropertyChanged();
            }
        }

        public bool ReverseEnabled
        {
            get => this._reverseEnabled;
            set
            {
                this._reverseEnabled = value;
                this.OnPropertyChanged();
            }
        }

        private bool _reverseEnabled;

        public override string ToString()
        {
            return this.ShowIp;
        }

        public ResultObject()
        {
            this.LastReceivedTime = DateTime.UtcNow.ToUnixTime();
            this.DetectionStamp = this.LastReceivedTime;
        }
    }
}
