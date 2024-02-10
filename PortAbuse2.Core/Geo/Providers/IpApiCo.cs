using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PortAbuse2.Core.Result;
using TiqUtils.Serialize;

namespace PortAbuse2.Core.Geo.Providers;

public class IpApiCo : IGeoService
{
    public string Name { get; } = "ipapi.co";

    public async Task<GeoData?> GetLocationByIp(string ip)
    {
        var loc = new GeoData();
        var url = "http://ipapi.co/" + ip + "/json";

        try
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:76.0) Gecko/20100101 Firefox/76.0");
                client.Timeout = TimeSpan.FromSeconds(10);
                var response = await client.GetStringAsync(url);
                var geoData = response.DeserializeDataFromString<IpInfo>();
                if (geoData != null)
                {
                    loc.Isp = geoData.Org;
                    loc.CountryCode = geoData.Country?.ToLower();
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

    private class IpInfo
    {
        [JsonProperty("ip")]
        public string Ip { get; set; }

        [JsonProperty("network")]
        public string Network { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; }

        [JsonProperty("region_code")]
        public string RegionCode { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("country_name")]
        public string CountryName { get; set; }

        [JsonProperty("country_code")]
        public string CountryCode { get; set; }

        [JsonProperty("country_code_iso3")]
        public string CountryCodeIso3 { get; set; }

        [JsonProperty("country_capital")]
        public string CountryCapital { get; set; }

        [JsonProperty("country_tld")]
        public string CountryTld { get; set; }

        [JsonProperty("continent_code")]
        public string ContinentCode { get; set; }

        [JsonProperty("in_eu")]
        public bool InEu { get; set; }

        [JsonProperty("postal")]
        public string Postal { get; set; }

        [JsonProperty("latitude")]
        public double Latitude { get; set; }

        [JsonProperty("longitude")]
        public double Longitude { get; set; }

        [JsonProperty("timezone")]
        public string Timezone { get; set; }

        [JsonProperty("utc_offset")]
        public string UtcOffset { get; set; }

        [JsonProperty("country_calling_code")]
        public string CountryCallingCode { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("currency_name")]
        public string CurrencyName { get; set; }

        [JsonProperty("languages")]
        public string Languages { get; set; }

        [JsonProperty("country_area")]
        public string CountryArea { get; set; }

        [JsonProperty("country_population")]
        public string CountryPopulation { get; set; }

        [JsonProperty("asn")]
        public string Asn { get; set; }

        [JsonProperty("org")]
        public string Org { get; set; }
    }
}