using System;
using System.Collections.Generic;
using PortAbuse2.Core.Parser;
using PortAbuse2.Core.Proto;

namespace PortAbuse2.Core.Listener
{
    internal class PortStorage
    {
        public PortStorage(IEnumerable<Port.Port> appPort)
        {
            this.UdpPorts = new HashSet<uint>();
            this.TcpPorts = new HashSet<uint>();
            this.UnknownPorts = new HashSet<uint>();

            foreach (var p in appPort)
            {
                switch (p.Protocol)
                {
                    case Protocol.Unknown:
                        this.UnknownPorts.Add(p.UPortNumber);
                        break;
                    case Protocol.Tcp:
                        this.TcpPorts.Add(p.UPortNumber);
                        break;
                    case Protocol.Udp:
                        this.UdpPorts.Add(p.UPortNumber);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private HashSet<uint> UdpPorts { get; }
        private HashSet<uint> TcpPorts { get; }
        private HashSet<uint> UnknownPorts { get; }

        public bool Any(PortInformation port)
        {
            switch (port.Protocol)
            {
                case Protocol.Unknown:
                    return this.UnknownPorts.Contains(port.DestinationPort) ||
                           this.UnknownPorts.Contains(port.SourcePort);
                case Protocol.Tcp:
                    return this.TcpPorts.Contains(port.DestinationPort) ||
                           this.TcpPorts.Contains(port.SourcePort);
                case Protocol.Udp:
                    return this.UdpPorts.Contains(port.DestinationPort) ||
                           this.UdpPorts.Contains(port.SourcePort);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}