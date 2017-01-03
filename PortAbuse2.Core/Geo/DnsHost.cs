using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using PortAbuse2.Core.Result;

namespace PortAbuse2.Core.Geo
{
    public static class DnsHost
    {
        public static void FillIpHost(ResultObject obj)
        {
            IPAddress ip = IPAddress.Parse(obj.ShowIp);
            string hostName;
            try
            {
                hostName = Dns.GetHostEntry(ip).HostName;
            }
            catch (Exception)
            {
                hostName = obj.ShowIp.Replace('.', '-') + ".NoHostName";
            }
            obj.Hostname = hostName;
        }
    }
}
