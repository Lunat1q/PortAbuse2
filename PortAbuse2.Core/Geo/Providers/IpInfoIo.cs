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
                            loc.Isp = geoData.org;
                            loc.CountryCode = geoData.country.ToLower();
                            loc.City = string.IsNullOrWhiteSpace(geoData.city) ? "Unknown" : $"{geoData.city}-{geoData.region}";
                            loc.Country = string.IsNullOrWhiteSpace(geoData.country) ? "Unknown" : geoData.country;
                            loc.Index = geoData.postal == "" ? "" : geoData.postal;
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
            return Name;
        }

        private class ProviderGeoData
        {
            public string ip { get; set; }
            public string hostname { get; set; }
            public string city { get; set; }
            public string region { get; set; }
            public string country { get; set; }
            public string loc { get; set; }
            public string org { get; set; }
            public string postal { get; set; }
        }
    }
}