using PortAbuse2.Core.Proto;

namespace PortAbuse2.Core.Port
{
    // ===============================================
    // The Port Class We're Going To Create A List Of
    // ===============================================
    public class Port
    {
        public string PortNumber => this.UPortNumber.ToString();
        public uint UPortNumber { get; set; }
        public Protocol Protocol { get; set; }

        public override string ToString() => $"{this.Protocol} : {this.PortNumber}";
    }
}
