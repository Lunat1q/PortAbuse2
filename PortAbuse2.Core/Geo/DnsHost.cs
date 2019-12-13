using System;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using PortAbuse2.Core.Result;

namespace PortAbuse2.Core.Geo
{
    public static class DnsHost
    {
        public static async void FillIpHost(ConnectionInformation obj, bool minimize)
        {
            var ip = IPAddress.Parse(obj.ShowIp);
            string hostName;
            try
            {
                hostName = (await Dns.GetHostEntryAsync(ip)).HostName;
            }
            catch (SocketException)
            {
                hostName = obj.ShowIp.Replace('.', '-') + ".NoHost";
            }
            obj.DetectedHostname = hostName;
            obj.Hostname = !minimize ? hostName : MinimizeHostname(hostName);
        }

        public static string MinimizeHostname(string hostName)
        {
            if (hostName != null && hostName.Length > 35)
                hostName = Regex.Replace(hostName, @"[\d-]", string.Empty) + "*";
            return hostName;
        }
    }
}
