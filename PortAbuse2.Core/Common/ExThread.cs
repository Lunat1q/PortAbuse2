using System;
using System.Threading;

namespace PortAbuse2.Core.Common
{
    public class ExThread
    {
        public Thread Td { get; }
        public DateTime TdTime { get; }
        public ExThread(Thread td, DateTime tdTime = new DateTime())
        {
            this.Td = td;
            this.TdTime = tdTime;
        }

    }
}
