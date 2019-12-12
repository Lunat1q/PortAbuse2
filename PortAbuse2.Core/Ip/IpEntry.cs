namespace PortAbuse2.Core.Ip
{
    public class IpEntry
    {
        public string LocIp { get; set; }
        public string RemIp { get; set; }
        public string RemHost { get; set; }
        public bool Marked { get; set; }
        public IpEntry(string locIp, string remIp, string remHost)
        {
            this.LocIp = locIp;
            this.RemIp = remIp;
            this.RemHost = remHost;
        }
        public IpEntry(string locIp, string remIp)
        {
            this.LocIp = locIp;
            this.RemIp = remIp;
        }
    }
}
