using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortAbuse2.Core.Geo
{
    public class GeoQueue
    {
        public string Ip { get; set; }
        public int GeoBase { get; set; }
        public bool RequestDone { get; set; }
        public bool RequestInProgress { get; set; }
        public GeoQueue(string ip, int geoBase)
        {
            Ip = ip;
            GeoBase = geoBase;
            RequestInProgress = true;
            RequestDone = false;
        }
    }
}
