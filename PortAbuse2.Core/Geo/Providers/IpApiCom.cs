﻿using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using PortAbuse2.Core.Result;

namespace PortAbuse2.Core.Geo.Providers;

public class IpApiCom : IGeoService
{
    public string Name { get; } = "ip-api.com";

    public async Task<GeoData?> GetLocationByIp(string ip)
    {
        var loc = new GeoData();
        var url = "http://ip-api.com/xml/" + ip;

        try
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd(
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:76.0) Gecko/20100101 Firefox/76.0");
                client.Timeout = TimeSpan.FromSeconds(10);
                var response = await client.GetStringAsync(url);

                var doc = XDocument.Parse(response);
                var geoData = doc.Element("query");
                if (geoData != null)
                {
                    var locCountry = geoData.Element("country")?.Value;
                    var locCity = geoData.Element("city")?.Value;
                    var zip = geoData.Element("zip")?.Value;
                    var isp = geoData.Element("isp")?.Value;
                    var countryCode = geoData.Element("countryCode")?.Value.ToLower();
                    loc.Isp = isp!;
                    loc.CountryCode = countryCode!;
                    loc.City = locCity == "" ? "Unknown" : locCity!;
                    loc.Country = locCountry == "" ? "Unknown" : locCountry!;
                    loc.Index = zip == "" ? "" : zip!;
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
}