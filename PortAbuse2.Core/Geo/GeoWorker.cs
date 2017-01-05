using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using PortAbuse2.Core.Result;
// ReSharper disable AssignNullToNotNullAttribute

namespace PortAbuse2.Core.Geo
{
    public static class GeoWorker
    {
        private static readonly Queue<GeoQueue> GeoQ = new Queue<GeoQueue>();
        private static int _geoRequests;
        private static bool _geoRunning;
        public static void InsertGeoDataQueue(ResultObject item, int i = 0)
        {
            var gq = new GeoQueue(item, i);
            GeoQ.Enqueue(gq);
            //IPGeoQue.Add(gq);

            if (!_geoRunning)
                Task.Run(DoGeoDataQueue);
        }

        private static async Task DoGeoDataQueue()
        {
            _geoRunning = true;
            while (GeoQ.Count > 0)
            {
                if (_geoRequests >= 10) continue;
                var gq = GeoQ.Dequeue();
                if (gq != null)
                {
                    GetGeoData(gq.Object, gq.GeoBase);
                    _geoRequests++;
                }
                await Task.Delay(100);
            }
            _geoRunning = false;
        }
        private static async void GetGeoData(ResultObject obj, int i = 0)
        {
            GeoData loc = null;
            switch (i)
            {
                case 0:
                    loc = await GetLocationByIp2(obj.ShowIp);
                    break;
                case 1:
                    loc = await GetLocationByIp3(obj.ShowIp);
                    break;
                case 2:
                    loc = await GetLocationByIp(obj.ShowIp);
                    break;
            }
            if (loc == null && i != 1)
                loc = await GetLocationByIp3(obj.ShowIp);
            if (loc == null && i != 2)
                loc = await GetLocationByIp(obj.ShowIp);
            if (loc == null && i != 0)
                loc = await GetLocationByIp2(obj.ShowIp);
            if (loc == null)
                loc = new GeoData()
                {
                    City = "Unknown",
                    Country = "Error",
                    Index = "000000"
                };
            obj.Geo.Merge(loc);
            _geoRequests--;
        }

        private static async Task<GeoData> GetLocationByIp(string ip)
        {
            var loc = new GeoData();
            var url = "http://api.2ip.com.ua/geo.xml?ip=" + ip;

            try
            {
                var request = WebRequest.Create(url);
                request.Timeout = 10000;
                using (var response = (HttpWebResponse)await request.GetResponseAsync())
                {
                    var encoding = response.CharacterSet != "" ? Encoding.GetEncoding(response.CharacterSet) : null;
                    using (var sr = encoding != null ? new StreamReader(response.GetResponseStream(), encoding) :
                                                           new StreamReader(response.GetResponseStream(), true))
                    {

                        var response2 = sr.ReadToEnd();
                        var doc = XDocument.Parse(response2);
                        var geoData = doc.Element("geo_api");
                        var locCountry = geoData?.Element("country_rus")?.Value;
                        var locCity = geoData?.Element("city_rus")?.Value;
                        var zip = geoData?.Element("zip_code")?.Value;
                        if (zip == "-") zip = "";
                        var countryCode = geoData?.Element("country_code")?.Value.ToLower();
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
        private static async Task<GeoData> GetLocationByIp2(string ip)
        {
            var loc = new GeoData();
            var url = "http://ip-api.com/xml/" + ip;

            try
            {
                var request = WebRequest.Create(url);
                request.Timeout = 10000;
                using (var response = (HttpWebResponse)await request.GetResponseAsync())
                {
                    var encoding = response.CharacterSet != "" ? Encoding.GetEncoding(response.CharacterSet) : null;
                    using (var sr = encoding != null ? new StreamReader(response.GetResponseStream(), encoding) :
                                                           new StreamReader(response.GetResponseStream(), true))
                    {
                        var response2 = sr.ReadToEnd();
                        var doc = XDocument.Parse(response2);
                        var geoData = doc.Element("query");
                        if (geoData != null)
                        {
                            var locCountry = geoData.Element("country")?.Value;
                            var locCity = geoData.Element("city")?.Value;
                            var zip = geoData.Element("zip")?.Value;
                            var isp = geoData.Element("isp")?.Value;
                            var countryCode = geoData.Element("countryCode")?.Value.ToLower();
                            loc.Isp = isp;
                            loc.CountryCode = countryCode;
                            loc.City = locCity == "" ? "Unknown" : locCity;
                            loc.Country = locCountry == "" ? "Unknown" : locCountry;
                            loc.Index = zip == "" ? "" : zip;
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
        private static async Task<GeoData> GetLocationByIp3(string ip)
        {
            var loc = new GeoData();
            var url = "http://freegeoip.net/xml/" + ip;

            try
            {
                var request = WebRequest.Create(url);
                request.Timeout = 10000;
                using (var response = (HttpWebResponse)await request.GetResponseAsync())
                {
                    var encoding = response.CharacterSet != "" ? Encoding.GetEncoding(response.CharacterSet) : null;
                    using (var sr = encoding != null ? new StreamReader(response.GetResponseStream(), encoding) :
                                                           new StreamReader(response.GetResponseStream(), true))
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
    }
}
