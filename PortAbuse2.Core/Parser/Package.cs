using System;
using System.Collections.Generic;
using PortAbuse2.Core.Proto;
using PortAbuse2.Core.Proto.Tcp;
using PortAbuse2.Core.Proto.Udp;

namespace PortAbuse2.Core.Parser
{
    public static class Package
    {
        public static IEnumerable<Tuple<Protocol,string>> GetPorts(IpHeader ipHeader)
        {
            switch (ipHeader.ProtocolType)
            {
                case Protocol.Tcp:

                    var tcpHeader = new TcpHeader(ipHeader.Data,              //IPHeader.Data stores the data being 
                                                                              //carried by the IP datagram
                                                        ipHeader.MessageLength);//Length of the data field                    

                    return new[] { Tuple.Create(Protocol.Tcp, tcpHeader.DestinationPort), Tuple.Create(Protocol.Tcp, tcpHeader.SourcePort) };

                case Protocol.Udp:

                    var udpHeader = new UdpHeader(ipHeader.Data,              //IPHeader.Data stores the data being 
                                                                              //carried by the IP datagram
                                                       ipHeader.MessageLength);//Length of the data field                    

                    return new[] { Tuple.Create(Protocol.Udp, udpHeader.DestinationPort), Tuple.Create(Protocol.Udp, udpHeader.SourcePort) };

                case Protocol.Unknown:
                    return null;
                default:
                    return null;
            }
        }
    }
}
