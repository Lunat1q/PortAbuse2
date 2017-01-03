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
        public Thread td { get; set; }
        public DateTime tdTime { get; set; }
        public int tdLiveTime { get; set; }
        public ExThread(Thread _td, DateTime _tdTime = new DateTime(), int _tdLiveTime = 0)
        {
            td = _td;
            tdTime = _tdTime;
            tdLiveTime = _tdLiveTime;
        }

    }
}
