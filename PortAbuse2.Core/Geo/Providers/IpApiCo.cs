using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;   
using PortAbuse2.Core.Result;
using TiqUtils.Serialize;

namespace PortAbuse2.Core.Geo.Providers
{
    public class IpApiCo : IGeoService
    {
        public string Name { get; } = "ipapi.co";
        public async Task<GeoData> GetLocationByIp(string ip)
        {
            var loc = new GeoData();
            var url = "http://ipapi.co/" + ip + "/json";

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
                        var geoData = Json.DeserializeDataFromString<IpApiCoData>(response2);
                        if (geoData != null)
                        {
                            loc.Isp = geoData.org;
                            loc.CountryCode = geoData.country.ToLower();
                            loc.City = string.IsNullOrWhiteSpace(geoData.city) ? "Unknown" : geoData.city;
                            loc.Country = string.IsNullOrWhiteSpace(geoData.country_name) ? "Unknown" : geoData.country_name;
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

        private class IpApiCoData
        {
            public string ip { get; set; }
            public string city { get; set; }
            public string region { get; set; }
            public string region_code { get; set; }
            public string country { get; set; }
            public string country_name { get; set; }
            public string postal { get; set; }
            public double latitude { get; set; }
            public double longitude { get; set; }
            public string timezone { get; set; }
            public string asn { get; set; }
            public string org { get; set; }
        }
    }
}