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
                            loc.Isp = geoData.Org;
                            loc.CountryCode = geoData.Country.ToLower();
                            loc.City = string.IsNullOrWhiteSpace(geoData.City) ? "Unknown" : geoData.City;
                            loc.Country = string.IsNullOrWhiteSpace(geoData.CountryName) ? "Unknown" : geoData.CountryName;
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

        private class IpApiCoData
        {
            public string Ip { get; set; }
            public string City { get; set; }
            public string Region { get; set; }
            public string RegionCode { get; set; }
            public string Country { get; set; }
            public string CountryName { get; set; }
            public string Postal { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public string Timezone { get; set; }
            public string Asn { get; set; }
            public string Org { get; set; }
        }
    }
}