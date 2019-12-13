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
                        if (data?.Data?.Geo != null)
                        {
                            var geoData = data.Data.Geo;
                            loc.Isp = geoData.Isp;
                            loc.CountryCode = geoData.Country_Code?.ToLower();
                            loc.City = string.IsNullOrWhiteSpace(geoData.City)
                                ? "Unknown"
                                : $"{geoData.City}";
                            loc.Country = string.IsNullOrWhiteSpace(geoData.Country_Name) ? "Unknown" : geoData.Country_Name;
                            loc.Index = geoData.PostalCode == "" ? "" : geoData.PostalCode;
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
            return this.Name;
        }

        private class Geo
        {
            public string Host { get; set; }
            public string Ip { get; set; }
            public string Rdns { get; set; }
            public string Asn { get; set; }
            public string Isp { get; set; }
            public string Country_Name { get; set; }
            public string Country_Code { get; set; }
            public string Region { get; set; }
            public string City { get; set; }
            public string PostalCode { get; set; }
            public string ContinentCode { get; set; }
            public string Latitude { get; set; }
            public string Longitude { get; set; }
            public string DmaCode { get; set; }
            public string AreaCode { get; set; }
            public string Timezone { get; set; }
            public string Datetime { get; set; }
        }

        private class Data
        {
            public Geo Geo { get; set; }
        }

        private class ProviderGeoData
        {
            public string Status { get; set; }
            public string Description { get; set; }
            public Data Data { get; set; }
        }
    }
}