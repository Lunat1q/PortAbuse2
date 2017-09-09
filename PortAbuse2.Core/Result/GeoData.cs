using PortAbuse2.Core.Common;

namespace PortAbuse2.Core.Result
{
    public class GeoData : PaNotified
    {
        private string _country;
        private string _city;
        private string _index;
        private string _countryCode;
        private string _isp;
        private bool _geoRequestEnqueued;

        public bool GeoRequestEnqueued
        {
            get => _geoRequestEnqueued;
            set
            {
                _geoRequestEnqueued = value; 
                OnPropertyChanged();
            }
        }

        public string Isp
        {
            get => _isp;
            set { _isp = value; OnPropertyChanged(); }
        }

        public string Country
        {
            get => _country;
            set
            {
                _country = value;
                OnPropertyChanged();
            }
        }

        public string City
        {
            get => _city;
            set
            {
                _city = value;
                OnPropertyChanged();
            }
        }

        public string Index
        {
            get => _index;
            set
            {
                _index = value;
                OnPropertyChanged();
            }
        }

        public string CountryCode
        {
            get => _countryCode;
            set
            {
                _countryCode = value;
                OnPropertyChanged();
            }
        }

        public string Result => $"{(string.IsNullOrEmpty(Index)?"" : $"[{Index}] - ")}{(string.IsNullOrEmpty(Country)? "" : $"{Country}")}{(string.IsNullOrEmpty(City)?"":$" - {City}")}{(string.IsNullOrEmpty(Isp) ? "" : $" [{Isp}]")}";

        public void Merge(GeoData geo)
        {
            City = geo.City;
            CountryCode = geo.CountryCode;
            Country = geo.Country;
            Index = geo.Index;
            Isp = geo.Isp;
            OnPropertyChangedByName(nameof(Result));
        }

        public void Reset()
        {
            City = string.Empty;
            CountryCode = string.Empty;
            Country = string.Empty;
            Index = string.Empty;
            Isp = string.Empty;
            GeoRequestEnqueued = false;
            OnPropertyChangedByName(nameof(Result));
        }
    }
}
