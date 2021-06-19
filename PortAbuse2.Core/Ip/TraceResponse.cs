using System.Net;
using System.Net.NetworkInformation;

namespace PortAbuse2.Common
{
    public readonly struct TraceResponse
    {
        public TraceResponse(IPStatus status, IPAddress address)
        {
            this.Status = status;
            this.Address = address;
        }

        public IPStatus Status { get; }

        public IPAddress Address { get; }

        public bool IsOk()
        {
            return this.Status == IPStatus.Success || this.Status == IPStatus.TtlExpired;
        }
    }
}