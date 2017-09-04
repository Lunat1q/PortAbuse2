using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Channels;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using PortAbuse2.Core.Listener;
using PortAbuse2.Core.Proto;
using PortAbuse2.Core.Result;

namespace PortAbuse2.Core.ApplicationExtensions
{
    public interface IApplicationExtension
    {
        IEnumerable<ResultObject> ResultObjectRef { set; }
        bool Active { get; set; }

        /// <summary>
        /// Should contain all possibile variation of exe name (w/o exe extension)
        /// </summary>
        IEnumerable<string> AppNames { get; }

        void Start();

        void Stop();

        /// <summary>
        /// Event handler for CoreReceiver Received event.
        /// </summary>
        /// <param name="ipDest"></param>
        /// <param name="ipSource"></param>
        /// <param name="data"></param>
        /// <param name="direction"></param>
        /// <param name="resultobject"></param>
        /// <param name="protocol"></param>
        /// <param name="initialToPackage"></param>
        void PackageReceived(IPAddress ipDest, IPAddress ipSource, byte[] data, bool direction,
            ResultObject resultobject, IEnumerable<Tuple<Protocol, ushort>> protocol);
    }
}
