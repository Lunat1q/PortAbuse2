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


        public string Isp
        {
            get { return _isp; }
            set { _isp = value; OnPropertyChanged(); }
        }

        public string Country
        {
            get { return _country; }
            set
            {
                _country = value;
                OnPropertyChanged();
            }
        }

        public string City
        {
            get { return _city; }
            set
            {
                _city = value;
                OnPropertyChanged();
            }
        }

        public string Index
        {
            get { return _index; }
            set
            {
                _index = value;
                OnPropertyChanged();
            }
        }

        public string CountryCode
        {
            get { return _countryCode; }
            set
            {
                _countryCode = value;
                OnPropertyChanged();
            }
        }

        public string Result => $"{(string.IsNullOrEmpty(Index)?"" : $"[{Index}]")}{(string.IsNullOrEmpty(Country)? "" : $" - {Country}")}{(string.IsNullOrEmpty(City)?"":$" - {City}")}{(string.IsNullOrEmpty(Isp) ? "" : $" [{Isp}]")}";

        public void Merge(GeoData geo)
        {
            City = geo.City;
            CountryCode = geo.CountryCode;
            Country = geo.Country;
            Index = geo.Index;
            Isp = geo.Isp;
            OnPropertyChangedByName("Result");
        }
    }
}
