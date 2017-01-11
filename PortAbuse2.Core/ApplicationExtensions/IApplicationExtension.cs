using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using PortAbuse2.Core.Listener;

namespace PortAbuse2.Core.ApplicationExtensions
{
    public interface IApplicationExtension
    {
        string[] AppNames { get; }
        bool Active { get; set; }

        void Start();

        void Stop();

        CoreReceiver Receiver { get; set; }
    }
}
