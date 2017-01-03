using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using PortAbuse2.Core.Common;

namespace PortAbuse2.Core.Result
{
    public class ResultObject : PaNotified
    {
        public IPAddress SourceAddress;
        public IPAddress DestinationAddress;

        public string SourceIp => SourceAddress.ToString();
        public string DestIp => DestinationAddress.ToString();

        public string ShowIp => !Reverse ? SourceIp : DestIp;

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

        private bool _reverse;

        public bool Reverse
        {
            get
            {
                return _reverse;
            }
            set
            {
                _reverse = value;
                OnPropertyChanged();
            }
        }

        private bool _blocked;
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
