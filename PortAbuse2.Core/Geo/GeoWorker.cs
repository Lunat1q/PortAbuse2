using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using PortAbuse2.Core.ApplicationExtensions;
using PortAbuse2.Core.Result;
// ReSharper disable AssignNullToNotNullAttribute

namespace PortAbuse2.Core.Geo
{
    public static class GeoWorker
    {
        private static readonly ConcurrentQueue<GeoQueue> GeoQ = new ConcurrentQueue<GeoQueue>();
        private static int _geoRequests;
        private static bool _geoRunning;
        public static List<IGeoService> GeoProviders = new List<IGeoService>();
        private static IGeoService _selectedGeoService;

        static GeoWorker()
        {
            var iExtension = typeof(IGeoService);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => iExtension.IsAssignableFrom(p) && !p.IsInterface);
            foreach (var type in types)
            {
                var instance = Activator.CreateInstance(type) as IGeoService;
                if (instance != null)
                    GeoProviders.Add(instance);
            }
            _selectedGeoService = GeoProviders.First();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item">result item</param>
        /// <param name="providerName">empty for current selected geo provider</param>
        public static void InsertGeoDataQueue(ResultObject item, string providerName = "")
        {
            if (item.Geo.GeoRequestEnqueued) return;
            item.Geo.GeoRequestEnqueued = true;
            var gq = new GeoQueue(item, providerName);
            GeoQ.Enqueue(gq);

            if (!_geoRunning)
                Task.Run(DoGeoDataQueue);
        }

        private static async Task DoGeoDataQueue()
        {
            _geoRunning = true;
            while (GeoQ.Count > 0)
            {
                if (_geoRequests >= 5) continue;
                if (GeoQ.TryDequeue(out GeoQueue gq))
                {
                    if (gq != null)
                    {
                        _geoRequests++;
                        await GetGeoData(gq.Object, gq.GeoProvider);
                    }
                    await Task.Delay(100);
                }
            }
            _geoRunning = false;
        }
        private static async Task GetGeoData(ResultObject obj, string providerName = "")
        {
            GeoData loc;
            if (providerName == string.Empty)
            {
                loc = await _selectedGeoService.GetLocationByIp(obj.ShowIp);
            }
            else
            {
                var provider = GeoProviders.FirstOrDefault(x => x.Name == providerName);
                if (provider != null)
                {
                    loc = await provider.GetLocationByIp(obj.ShowIp);
                }
                else
                {
                    await GetGeoData(obj);
                    return;
                }

            }

            if (loc == null)
            {
                foreach (var provider in GeoProviders)
                {
                    loc = await provider.GetLocationByIp(obj.ShowIp);
                    if (loc != null) break;
                }
            }

            if (loc == null)
                loc = new GeoData
                {
                    City = "Unknown",
                    Country = "Error",
                    Index = "000000"
                };
            obj.Geo.Merge(loc);
            _geoRequests--;
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

        public static IGeoService SelectProviderByName(string defaultGeoProvider)
        {
            _selectedGeoService = string.IsNullOrWhiteSpace(defaultGeoProvider)
                ? GeoProviders.FirstOrDefault()
                : GeoProviders.FirstOrDefault(x => x.Name == defaultGeoProvider);
            return _selectedGeoService;
        }

        public static void SelectProviderByObject(IGeoService item)
        {
            if (GeoProviders.Contains(item))
            {
                _selectedGeoService = item;
            }
        }
    }
}
