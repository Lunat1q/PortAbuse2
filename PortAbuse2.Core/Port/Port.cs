﻿namespace PortAbuse2.Core.Port
{
    // ===============================================
    // The Port Class We're Going To Create A List Of
    // ===============================================
    public class Port
    {
        public string PortNumber => UPortNumber.ToString();
        public uint UPortNumber { get; set; }
        public string Protocol { get; set; }

        public override string ToString() => $"{Protocol} : {PortNumber}";
    }
}
