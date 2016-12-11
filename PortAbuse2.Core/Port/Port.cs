namespace PortAbuse2.Core.Port
{
    // ===============================================
    // The Port Class We're Going To Create A List Of
    // ===============================================
    public class Port
    {
        public string Name => $"{ProcessName} ({Protocol} port {PortNumber})";
        public string PortNumber { get; set; }
        public string ProcessName { get; set; }
        public string Protocol { get; set; }
    }
}
