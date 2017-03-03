namespace PortAbuse2.Core.Port
{
    // ===============================================
    // The Port Class We're Going To Create A List Of
    // ===============================================
    public class Port
    {
        public string PortNumber { get; set; }
        public string Protocol { get; set; }
        public string ProcessName { get; set; }

        public override string ToString() => $"{ProcessName} : {Protocol} : {PortNumber}";
    }
}
