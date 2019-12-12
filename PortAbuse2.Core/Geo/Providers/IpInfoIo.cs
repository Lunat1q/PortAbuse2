using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;   
using PortAbuse2.Core.Result;
using TiqUtils.Serialize;

namespace PortAbuse2.Core.Geo.Providers
{
    public class IpInfoIo : IGeoService
    {
        public string Name { get; } = "IpInfo.IO";
        public async Task<GeoData> GetLocationByIp(string ip)
        {
            var loc = new GeoData();
            var url = "https://ipinfo.io/" + ip + "/json";

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
                        var geoData = Json.DeserializeDataFromString<ProviderGeoData>(response2);
                        if (geoData != null)
                        {
                            loc.Isp = geoData.Org;
                            loc.CountryCode = geoData.Country.ToLower();
                            loc.City = string.IsNullOrWhiteSpace(geoData.City) ? "Unknown" : $"{geoData.City}-{geoData.Region}";
                            loc.Country = string.IsNullOrWhiteSpace(geoData.Country) ? "Unknown" : geoData.Country;
                            loc.Index = geoData.Postal == "" ? "" : geoData.Postal;
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
            return loc;
        }
        public override string ToString()
        {
            return this.Name;
        }

        private class ProviderGeoData
        {
            public string Ip { get; set; }
            public string Hostname { get; set; }
            public string City { get; set; }
            public string Region { get; set; }
            public string Country { get; set; }
            public string Loc { get; set; }
            public string Org { get; set; }
            public string Postal { get; set; }
        }
    }
}