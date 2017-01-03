using PortAbuse2.Core.Result;

namespace PortAbuse2.Core.Geo
{
    public class GeoQueue
    {
        public string Ip => Object.ShowIp;
        public int GeoBase { get; set; }
        public bool RequestDone { get; set; }
        public bool RequestInProgress { get; set; }
        public ResultObject Object { get; set; }
        public GeoQueue(ResultObject obj, int geoBase)
        {
            Object = obj;
            GeoBase = geoBase;
            RequestInProgress = true;
            RequestDone = false;
        }
    }
}
