using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortAbuse2.Core.Ip
{
    public class IpEntry
    {
        public string LocIp { get; set; }
        public string RemIp { get; set; }
        public string RemHost { get; set; }
        public bool Marked { get; set; }
        public IpEntry(string locIp, string remIp, string remHost)
        {
            LocIp = locIp;
            RemIp = remIp;
            RemHost = remHost;
        }
        public IpEntry(string locIp, string remIp)
        {
            LocIp = locIp;
            RemIp = remIp;
        }
    }
}
