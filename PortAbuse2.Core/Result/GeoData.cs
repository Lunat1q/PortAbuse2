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
            get => this._geoRequestEnqueued;
            set
            {
                this._geoRequestEnqueued = value;
                this.OnPropertyChanged();
            }
        }

        public string Isp
        {
            get => this._isp;
            set {
                this._isp = value;
                this.OnPropertyChanged(); }
        }

        public string Country
        {
            get => this._country;
            set
            {
                this._country = value;
                this.OnPropertyChanged();
            }
        }

        public string City
        {
            get => this._city;
            set
            {
                this._city = value;
                this.OnPropertyChanged();
            }
        }

        public string Index
        {
            get => this._index;
            set
            {
                this._index = value;
                this.OnPropertyChanged();
            }
        }

        public string CountryCode
        {
            get => this._countryCode;
            set
            {
                this._countryCode = value;
                this.OnPropertyChanged();
            }
        }

        public string Result => $"{(string.IsNullOrEmpty(this.Index)?"" : $"[{this.Index}] - ")}{(string.IsNullOrEmpty(this.Country)? "" : $"{this.Country}")}{(string.IsNullOrEmpty(this.City)?"":$" - {this.City}")}{(string.IsNullOrEmpty(this.Isp) ? "" : $" [{this.Isp}]")}";

        public void Merge(GeoData geo)
        {
            this.City = geo.City;
            this.CountryCode = geo.CountryCode;
            this.Country = geo.Country;
            this.Index = geo.Index;
            this.Isp = geo.Isp;
            this.OnPropertyChangedByName(nameof(this.Result));
        }

        public void Reset()
        {
            this.City = string.Empty;
            this.CountryCode = string.Empty;
            this.Country = string.Empty;
            this.Index = string.Empty;
            this.Isp = string.Empty;
            this.GeoRequestEnqueued = false;
            this.OnPropertyChangedByName(nameof(this.Result));
        }
    }
}
