using System.Net;
using PortAbuse2.Core.Common;

namespace PortAbuse2.Core.Result
{
    public class ResultObject : PaNotified
    {
        public IPAddress SourceAddress;
        public IPAddress DestinationAddress;

        public string SourceIp => SourceAddress.ToString();
        public string DestIp => DestinationAddress.ToString();

        public int PackagesReceived
        {
            get { return _packagesReceived; }
            set
            {
                _packagesReceived = value;
                OnPropertyChanged();
            }
        }

        public bool Hidden
        {
            get { return _hidden; }
            set
            {
                _hidden = value;
                OnPropertyChanged();
            }
        }

        public string ShowIp => From ? DestIp : SourceIp;

        public string ExtraInfo
        {
            get { return _extraInfo; }
            set
            {
                HaveExtraInfo = !string.IsNullOrEmpty(value);
                _extraInfo = value;
                OnPropertyChanged();
            }
        }

        public bool HaveExtraInfo
        {
            get { return _haveExtraInfo; }
            set
            {
                _haveExtraInfo = value;
                OnPropertyChanged(); 
                
            }
        }

        private string _hostname;
        public string Hostname
        {
            get
            {
                return _hostname; }
            set
            {
                _hostname = value;
                OnPropertyChanged();
            }
        }

        public AppEntry Application { get; set; }

        private GeoData _geo;

        public GeoData Geo
        {
            get { return _geo ?? (_geo = new GeoData()); }
            set
            {
                _geo = value;
                OnPropertyChanged();
            }
        }

        private bool _from;

        public bool From
        {
            get
            {
                return _from;
            }
            set
            {
                _from = value;
                OnPropertyChanged();
            }
        }

        private bool _blocked;
        private int _packagesReceived;
        private string _extraInfo;
        private bool _haveExtraInfo;
        private bool _hidden;

        public bool Blocked
        {
            get
            {
                return _blocked;
            }
            set
            {
                _blocked = value;
                OnPropertyChanged();
            }
        }
    }
}
