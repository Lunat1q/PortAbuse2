using System;
using System.Collections.Generic;
using System.Net;
using PortAbuse2.Core.Parser;
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

        void PackageReceived(IPAddress ipDest, IPAddress ipSource, byte[] data, bool direction,
            ResultObject resultobject, PortInformation portInfo);
    }
}
