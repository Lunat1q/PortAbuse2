using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using PortAbuse2.Core.Result;
using TiqUtils.Serialize;

namespace PortAbuse2.Core.Geo.Providers;

public class KeyCdnCom : IGeoService
{
    private static int _requestCounter;
    public string Name { get; } = "KeyCdn.com";

    public async Task<GeoData?> GetLocationByIp(string ip)
    {
        var loc = new GeoData();
        var url = "https://tools.keycdn.com/geo.json?host=" + ip;

        if (_requestCounter++ > 0)
            await Task.Delay(_requestCounter * 1000);
        try
        {

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("keycdn-tools github.com");
                client.Timeout = TimeSpan.FromSeconds(10);
                var response = await client.GetStringAsync(url);
                if (string.IsNullOrWhiteSpace(response)) return null;

                var data = response.DeserializeDataFromString<ProviderGeoData>();
                if (data?.Data?.Geo != null)
                {
                    var geoData = data.Data.Geo;
                    loc.Isp = geoData.Isp;
                    loc.CountryCode = geoData.Country_Code?.ToLower();
                    loc.City = string.IsNullOrWhiteSpace(geoData.City)
                        ? "Unknown"
                        : $"{geoData.City}";
                    loc.Country = string.IsNullOrWhiteSpace(geoData.Country_Name) ? "Unknown" : geoData.Country_Name;
                    loc.Index = string.IsNullOrWhiteSpace(geoData.Postal_Code) ? "" : geoData.Postal_Code;
                }
                else
                {
                    loc = null;
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
        return Name;
    }

    private class Geo
    {
        public string Host { get; set; }
        public string Ip { get; set; }
        public string Rdns { get; set; }
        public string Asn { get; set; }
        public string Isp { get; }
        public string Country_Name { get; }
        public string Country_Code { get; }
        public string Region { get; set; }
        public string City { get; }
        public string Postal_Code { get; }
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
        public Geo Geo { get; }
    }

    private class ProviderGeoData
    {
        public string Status { get; set; }
        public string Description { get; set; }
        public Data Data { get; }
    }
}