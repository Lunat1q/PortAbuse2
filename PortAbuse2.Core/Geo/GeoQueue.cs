using PortAbuse2.Core.Result;

namespace PortAbuse2.Core.Geo
{
    public class GeoQueue
    {
        public string Ip => Object.ShowIp;
        public string GeoProvider { get; }
        public bool RequestDone { get; }
        public bool RequestInProgress { get; }
        public ResultObject Object { get; }
        public GeoQueue(ResultObject obj, string providerName)
        {
            Object = obj;
            GeoProvider = providerName;
            RequestInProgress = true;
            RequestDone = false;
        }
    }
}
