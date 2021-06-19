using PortAbuse2.Core.Trace;

namespace PortAbuse2.Core.Geo
{
    public class GeoQueueTrace : GeoQueueBase
    {
        public GeoQueueTrace(TraceEntry obj, string providerName) : base(obj, providerName)
        {
            this.Ip = obj.Address.ToString();
        }
    }
}