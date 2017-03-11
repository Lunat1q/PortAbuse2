using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using PortAbuse2.Core.Listener;
using PortAbuse2.Core.Proto;
using PortAbuse2.Core.Result;

namespace PortAbuse2.Core.ApplicationExtensions
{
    public sealed class WarframeApp : IApplicationExtension
    {
        public string[] AppNames => new[] {"Warframe.x64", "Warframe"};
        public bool Active { get; set; }

        private async Task Worker()
        {
            if (Receiver == null) return;
            while (Active)
            {
                var visible = Receiver.ResultObjects.Where(x=>!x.Old && !x.Hidden).OrderBy(x=>x.DetectionStamp);
                var i = 2;
                foreach (var hosts in visible)
                {
                    hosts.ExtraInfo = i.ToString();
                    i++;
                }
                await Task.Delay(2000);
            }
        }

        public void Stop()
        {
            Active = false;
            Receiver.Received -= ReceiverOnReceived;
        }

        private void ReceiverOnReceived(IPAddress ipDest, IPAddress ipSource, byte[] data, bool direction, ResultObject resultobject, IEnumerable<Tuple<string, string>> protocol)
        {
            if (!resultobject.ReverseEnabled || !Active) return;
            if (direction) return;
            var protocolTo = protocol.LastOrDefault();
            if (protocolTo == null) return;
            int port;
            if (int.TryParse(protocolTo.Item2, out port) && protocolTo.Item1 == "UDPv4")
            {
                SendWithUdp(ipSource, data, port);
            }
        }

        private void SendWithUdp(IPAddress ip, byte[] data, int port)
        {
            Receiver.SendToUdp(ip, data, port);
        }


        public void Start()
        {
            Active = true;
            Task.Run(Worker);
            Receiver.Received += ReceiverOnReceived;
        }

        public CoreReceiver Receiver { get; set; }
    }
}
