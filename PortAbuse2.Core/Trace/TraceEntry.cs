using System.Net;
using PortAbuse2.Core.Result;

namespace PortAbuse2.Core.Trace
{
    public class TraceEntry : HostInformation
    {
        private IPAddress _address;
        private string _hostname;

        public TraceEntry(int traceStepNumber, IPAddress address)
        {
            this.TraceStepNumber = traceStepNumber;
            this._address = address;
        }

        public int TraceStepNumber { get; }

        public IPAddress Address
        {
            get => this._address;
            set
            {
                this._address = value;
                this.OnPropertyChanged();
            }
        }
        public string Hostname
        {
            get => this._hostname;
            set
            {
                this._hostname = value;
                this.OnPropertyChanged();
            }
        }
    }
}