using PortAbuse2.Core.Common;

namespace PortAbuse2.Core.Trace
{
    public class PingEntry : PaNotified
    {
        private long pingTime;

        public PingEntry(long pingTime)
        {
            this.pingTime = pingTime;
        }

        public long PingTime
        {
            get => this.pingTime;
            set
            {
                this.pingTime = value;
                this.OnPropertyChanged();
            }
        }
    }
}