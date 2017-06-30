using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PortAbuse2.Core.Common
{
    public class ExThread
    {
        public Thread Td { get; }
        public DateTime TdTime { get; }
        private int TdLiveTime { get; }
        public ExThread(Thread td, DateTime tdTime = new DateTime(), int tdLiveTime = 0)
        {
            Td = td;
            TdTime = tdTime;
            TdLiveTime = tdLiveTime;
        }

    }
}
