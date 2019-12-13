using PortAbuse2.Core.Result;

namespace PortAbuse2.Core.Geo
{
    public class GeoQueue
    {
        public string Ip => this.Object.ShowIp;
        public string GeoProvider { get; }
        public bool RequestDone { get; }
        public bool RequestInProgress { get; }
        public ConnectionInformation Object { get; }
        public GeoQueue(ConnectionInformation obj, string providerName)
        {
            this.Object = obj;
            this.GeoProvider = providerName;
            this.RequestInProgress = true;
            this.RequestDone = false;
        }
    }
}
