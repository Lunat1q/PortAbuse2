using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace PortAbuse2.Core.Ip
{
    public static class IpInterface
    {
        public static IEnumerable<string> GetIpInterfaces()
        {
            var ipList = new List<string>();
            var hosyEntry = Dns.GetHostEntry((Dns.GetHostName()));
            if (hosyEntry.AddressList.Length <= 0) return ipList;
            ipList.AddRange(
                from ip in hosyEntry.AddressList
                where ip.AddressFamily != AddressFamily.InterNetworkV6
                select ip.ToString());
            return ipList;
        }
    }
}
