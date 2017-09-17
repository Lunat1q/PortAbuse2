using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;   
using PortAbuse2.Core.Result;
using TiqUtils.Serialize;

namespace PortAbuse2.Core.Geo.Providers
{
    public class KeyCdnCom : IGeoService
    {
        private static int _requestCounter;
        public string Name { get; } = "KeyCdn.com";
        public async Task<GeoData> GetLocationByIp(string ip)
        {
            var loc = new GeoData();
            var url = "https://tools.keycdn.com/geo.json?host=" + ip;

            if (_requestCounter++ > 0)
                await Task.Delay(_requestCounter * 1000);
            try
            {
                var request = WebRequest.Create(url);
                request.Timeout = 10000;
                using (var response = (HttpWebResponse) await request.GetResponseAsync())
                {
                    if (response == null) return null;
                    var encoding = !string.IsNullOrWhiteSpace(response.CharacterSet)
                        ? Encoding.GetEncoding(response.CharacterSet)
                        : null;
                    var responseStream = response.GetResponseStream();
                    if (responseStream == null) return null;
                    using (var sr = encoding != null
                        ? new StreamReader(responseStream, encoding)
                        : new StreamReader(responseStream, true))
                    {
                        var response2 = sr.ReadToEnd();
                        var data = Json.DeserializeDataFromString<ProviderGeoData>(response2);
                        if (data?.data?.geo != null)
                        {
                            var geoData = data.data.geo;
                            loc.Isp = geoData.isp;
                            loc.CountryCode = geoData.country_code?.ToLower();
                            loc.City = string.IsNullOrWhiteSpace(geoData.city)
                                ? "Unknown"
                                : $"{geoData.city}";
                            loc.Country = string.IsNullOrWhiteSpace(geoData.country_name) ? "Unknown" : geoData.country_name;
                            loc.Index = geoData.postal_code == "" ? "" : geoData.postal_code;
                        }
                        else
                        {
                            loc = null;
                        }

                    }
                }
            }
            catch
            {
                loc = null;
            }
            finally
            {
                _requestCounter--;
                await Task.Delay(1000);
            }
            return loc;
        }
        public override string ToString()
        {
            return Name;
        }

        private class Geo
        {
            public string host { get; set; }
            public string ip { get; set; }
            public string rdns { get; set; }
            public string asn { get; set; }
            public string isp { get; set; }
            public string country_name { get; set; }
            public string country_code { get; set; }
            public string region { get; set; }
            public string city { get; set; }
            public string postal_code { get; set; }
            public string continent_code { get; set; }
            public string latitude { get; set; }
            public string longitude { get; set; }
            public string dma_code { get; set; }
            public string area_code { get; set; }
            public string timezone { get; set; }
            public string datetime { get; set; }
        }

        private class Data
        {
            public Geo geo { get; set; }
        }

        private class ProviderGeoData
        {
            public string status { get; set; }
            public string description { get; set; }
            public Data data { get; set; }
        }
    }
}