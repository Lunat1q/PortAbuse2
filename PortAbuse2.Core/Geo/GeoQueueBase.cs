using PortAbuse2.Core.Result;

namespace PortAbuse2.Core.Geo
{
    public abstract class GeoQueueBase
    {
        public string Ip { get; protected set; }
        public string GeoProvider { get; protected set; }
        public bool RequestDone { get; protected set; }
        public bool RequestInProgress { get; protected set; }
        public HostInformation Object { get; protected set; }

        protected GeoQueueBase(HostInformation obj, string providerName)
        {
            this.Object = obj;
            this.GeoProvider = providerName;
            this.RequestInProgress = true;
            this.RequestDone = false;
        }
    }
}