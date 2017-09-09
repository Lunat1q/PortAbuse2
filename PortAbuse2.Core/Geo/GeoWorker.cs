using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PortAbuse2.Core.Result;

// ReSharper disable AssignNullToNotNullAttribute

namespace PortAbuse2.Core.Geo
{
    public static class GeoWorker
    {
        private static readonly ConcurrentQueue<GeoQueue> GeoQ = new ConcurrentQueue<GeoQueue>();
        private static int _geoRequests;
        private static bool _geoRunning;
        public static readonly List<IGeoService> GeoProviders = new List<IGeoService>();
        private static IGeoService _selectedGeoService;
        private static Task _geoTask;
        private static readonly ReaderWriterLockSlim Rwl = new ReaderWriterLockSlim();

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

            Rwl.EnterWriteLock();
            if (!_geoRunning || _geoTask == null || _geoTask.IsCanceled || _geoTask.IsCompleted || _geoTask.IsFaulted)
            {
                _geoRunning = true;
                _geoTask = Task.Run(DoGeoDataQueue);
            }
            Rwl.ExitWriteLock();
        }

        private static async Task DoGeoDataQueue()
        {
            try
            {
                while (GeoQ.Count > 0)
                {
                    if (_geoRequests >= 5) continue;
                    if (GeoQ.TryDequeue(out GeoQueue gq))
                    {
                        if (gq != null)
                        {
                            _geoRequests++;
                            GetGeoData(gq.Object, gq.GeoProvider);
                        }
                        await Task.Delay(200);
                    }
                }
            }
            catch (Exception)
            {
                //ignore
            }
            finally
            {
                _geoRunning = false;
            }
        }

        private static async void GetGeoData(ResultObject obj, string providerName = "")
        {
            IGeoService provider;
            if (providerName == string.Empty)
            {
                provider = _selectedGeoService;
            }
            else
            {
                provider = GeoProviders.FirstOrDefault(x => x.Name == providerName) ?? _selectedGeoService;
            }
            var loc = await provider.GetLocationByIp(obj.ShowIp);

            if (loc == null)
            {
                foreach (var prov in GeoProviders.Where(x => x != provider))
                {
                    loc = await prov.GetLocationByIp(obj.ShowIp);
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