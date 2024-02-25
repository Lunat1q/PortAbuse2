using System;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PortAbuse2.Core.Result;
using PortAbuse2.Core.Trace;

namespace PortAbuse2.Core.Geo
{
    public static class DnsHost
    {
        public static async void FillIpHost(ConnectionInformation? obj, bool minimize)
        {
            var hostName = await GetHostName(obj.ShowIp);
            obj.DetectedHostname = hostName;
            obj.Hostname = !minimize ? hostName : MinimizeHostname(hostName);
        }

        private static async Task<string> GetHostName(IPAddress ip)
        {
            string hostName;
            try
            {
                hostName = (await Dns.GetHostEntryAsync(ip)).HostName;
            }
            catch (SocketException)
            {
                hostName = ip.ToString().Replace('.', '-') + ".NoHost";
            }

            return hostName;
        }

        public static string MinimizeHostname(string hostName)
        {
            if (hostName != null && hostName.Length > 35)
                hostName = Regex.Replace(hostName, @"[\d-]", string.Empty) + "*";
            return hostName;
        }

        public static async void FillIpHost(TraceEntry? traceEntry)
        {
            var hostName = await GetHostName(traceEntry.Address);
            traceEntry.Hostname = hostName;
        }
    }
}
