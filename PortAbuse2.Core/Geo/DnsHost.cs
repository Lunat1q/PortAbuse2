using System;
using System.Net;
using PortAbuse2.Core.Result;

namespace PortAbuse2.Core.Geo
{
    public static class DnsHost
    {
        public static async void FillIpHost(ResultObject obj)
        {
            var ip = IPAddress.Parse(obj.ShowIp);
            string hostName;
            try
            {
                hostName = (await Dns.GetHostEntryAsync(ip)).HostName;
            }
            catch (Exception)
            {
                hostName = obj.ShowIp.Replace('.', '-') + ".NoHost";
            }
            obj.Hostname = hostName;
        }
    }
}
