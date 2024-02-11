using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PortAbuse2.Core.Result;
using PortAbuse2.Core.Trace;

// ReSharper disable AssignNullToNotNullAttribute

namespace PortAbuse2.Core.Geo;

public static class GeoWorker
{
    private static readonly ConcurrentQueue<GeoQueueBase?> GeoQ = new();
    private static int _geoRequests;
    private static bool _geoRunning;
    public static readonly List<IGeoService> GeoProviders = new();
    private static IGeoService _selectedGeoService;
    private static Task? _geoTask;
    private static readonly ReaderWriterLockSlim Rwl = new();

    static GeoWorker()
    {
        var iExtension = typeof(IGeoService);
        var types = AppDomain.CurrentDomain.GetAssemblies()
                             .SelectMany(s => s.GetTypes())
                             .Where(p => iExtension.IsAssignableFrom(p) && !p.IsInterface);
        foreach (var type in types)
        {
            if (Activator.CreateInstance(type) is IGeoService instance)
            {
                GeoProviders.Add(instance);
            }
        }

        _selectedGeoService = GeoProviders.First();
    }

    /// <summary>
    /// </summary>
    /// <param name="item">result item</param>
    /// <param name="providerName">empty for current selected geo provider</param>
    public static void InsertGeoDataQueue(ConnectionInformation? item, string providerName = "")
    {
        if (item.Geo.GeoRequestEnqueued)
        {
            return;
        }

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
            while (!GeoQ.IsEmpty)
            {
                if (_geoRequests >= 5)
                {
                    continue;
                }

                if (GeoQ.TryDequeue(out var gq))
                {
                    if (gq != null)
                    {
                        _geoRequests++;
                        GetGeoData(gq, gq.GeoProvider);
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

    private static async void GetGeoData(GeoQueueBase? geoData, string providerName = "")
    {
        if (geoData == null)
        {
            return;
        }

        IGeoService provider;
        if (providerName == string.Empty)
        {
            provider = _selectedGeoService;
        }
        else
        {
            provider = GeoProviders.FirstOrDefault(x => x.Name == providerName) ?? _selectedGeoService;
        }

        var loc = await provider.GetLocationByIp(geoData.Ip);

        if (loc == null)
        {
            foreach (var prov in GeoProviders.Where(x => x != provider))
            {
                loc = await prov.GetLocationByIp(geoData.Ip);
                if (loc != null)
                {
                    break;
                }
            }
        }

        loc ??= new GeoData
        {
            City = "Unknown",
            Country = "Error",
            Index = "000000"
        };
        geoData.Object.Geo.Merge(loc);
        _geoRequests--;
    }

    public static IGeoService? SelectProviderByName(string defaultGeoProvider)
    {
        _selectedGeoService = string.IsNullOrWhiteSpace(defaultGeoProvider)
            ? GeoProviders.First()
            : GeoProviders.First(x => x.Name == defaultGeoProvider);
        return _selectedGeoService;
    }

    public static void SelectProviderByObject(IGeoService item)
    {
        if (GeoProviders.Contains(item))
        {
            _selectedGeoService = item;
        }
    }

    public static void InsertGeoDataQueue(TraceEntry? traceEntry)
    {
        if (traceEntry.Geo.GeoRequestEnqueued)
        {
            return;
        }

        traceEntry.Geo.GeoRequestEnqueued = true;
        var gq = new GeoQueueTrace(traceEntry, string.Empty);
        GeoQ.Enqueue(gq);

        Rwl.EnterWriteLock();

        if (!_geoRunning || _geoTask == null || _geoTask.IsCanceled || _geoTask.IsCompleted || _geoTask.IsFaulted)
        {
            _geoRunning = true;
            _geoTask = Task.Run(DoGeoDataQueue);
        }

        Rwl.ExitWriteLock();
    }
}