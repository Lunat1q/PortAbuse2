using PortAbuse2.Core.Result;

namespace PortAbuse2.Core.Geo
{
    public class GeoQueue : GeoQueueBase
    {
        public GeoQueue(ConnectionInformation obj, string providerName) : base(obj, providerName)
        {
            this.Ip = obj.ShowIp.ToString();
        }
    }
}
