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

        public string SourceIp => SourceAddress.ToString();
        public string DestIp => DestinationAddress.ToString();

        public bool Resolved { get; set; } = false;


        public bool ForceShow
        {
            get => _forceShow;
            set
            {
                _forceShow = value;
                OnPropertyChanged();
                OnPropertyChangedByName(nameof(Hidden));
                OnPropertyChangedByName(nameof(ForseShown));
                OnPropertyChangedByName(nameof(HiddenProp));
            }
        }

        public bool ForseShown => _forceShow && _hidden;

        public int PackagesReceived
        {
            get => _packagesReceived;
            set
            {
                _packagesReceived = value;
                LastReceivedTime = DateTime.UtcNow.ToUnixTime();
                OnPropertyChanged();
            }
        }

        public long DataTransfered
        {
            get => _dataTransfered;
            set
            {
                _dataTransfered = value;
                OnPropertyChanged();
            }
        }

        public bool Old
        {
            get => _old;
            set
            {
                _old = value;
                OnPropertyChanged();
            }
        }

        public bool HiddenProp
        {
            get => _hidden && !_forceShow;
            set
            {
                _hidden = value;
                OnPropertyChanged();
                OnPropertyChangedByName(nameof(Hidden));
                OnPropertyChangedByName(nameof(ForseShown));
            }
        }

        public bool Hidden
        {
            get => _hidden;
            set
            {
                _hidden = value;
                OnPropertyChanged();
                OnPropertyChangedByName(nameof(HiddenProp));
                OnPropertyChangedByName(nameof(ForseShown));
            }
        }

        public long LastReceivedTime { get; set; }

        public long DetectionStamp { get; set; }

        public string ShowIp => From ? DestIp : SourceIp;

        public string ExtraInfo
        {
            get => _extraInfo;
            set
            {
                HaveExtraInfo = !string.IsNullOrEmpty(value);
                _extraInfo = value;
                OnPropertyChanged();
            }
        }

        public bool HaveExtraInfo
        {
            get => _haveExtraInfo;
            set
            {
                _haveExtraInfo = value;
                OnPropertyChanged(); 
                
            }
        }

        private string _hostname;
        private bool _forceShow;
        public string DetectedHostname { get; set; }

        public string Hostname
        {
            get => _hostname;
            set
            {
                _hostname = value;
                OnPropertyChanged();
            }
        }

        public AppEntry Application { get; set; }

        public GeoData Geo
        {
            get => _geo ?? (_geo = new GeoData());
            set
            {
                _geo = value;
                OnPropertyChanged();
            }
        }

      

        public bool From
        {
            get => _from;
            set
            {
                _from = value;
                OnPropertyChanged();
            }
        }

        public bool Blocked
        {
            get => _blocked;
            set
            {
                _blocked = value;
                OnPropertyChanged();
            }
        }

        public bool ReverseEnabled
        {
            get => _reverseEnabled;
            set
            {
                _reverseEnabled = value;
                OnPropertyChanged();
            }
        }

        private bool _reverseEnabled;

        public override string ToString()
        {
            return ShowIp;
        }

        public ResultObject()
        {
            LastReceivedTime = DateTime.UtcNow.ToUnixTime();
            DetectionStamp = LastReceivedTime;
        }
    }
}
