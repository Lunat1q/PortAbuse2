using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using PortAbuse2.Core.Result;

namespace PortAbuse2.Core.Geo.Providers
{
    public class FreeGeoIpNet : IGeoService
    {
        public string Name { get; } = "freegeoip.net";
        public async Task<GeoData> GetLocationByIp(string ip)
        {
            var loc = new GeoData();
            var url = "http://freegeoip.net/xml/" + ip;

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
                        var doc = XDocument.Parse(response2);
                        var geoData = doc.Element("Response");
                        var locCountry = geoData?.Element("CountryName")?.Value;
                        var locCity = geoData?.Element("City")?.Value;
                        var zip = geoData?.Element("ZipCode")?.Value;
                        var countryCode = geoData?.Element("CountryCode")?.Value.ToLower();
                        loc.CountryCode = countryCode;
                        loc.City = locCity == "" ? "Unknown" : locCity;
                        loc.Country = locCountry == "" ? "Unknown" : locCountry;
                        loc.Index = zip == "" ? "" : zip;
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
    }
}