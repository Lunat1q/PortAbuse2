using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PortAbuse2.Core.Proto;
using PortAbuse2.Core.Proto.Tcp;
using PortAbuse2.Core.Proto.Udp;

namespace PortAbuse2.Core.Parser
{
    public static class Package
    {
        public static IEnumerable<Tuple<string,string>> GetPorts(IpHeader ipHeader)
        {
            switch (ipHeader.ProtocolType)
            {
                case Protocol.Tcp:

                    var tcpHeader = new TcpHeader(ipHeader.Data,              //IPHeader.Data stores the data being 
                                                                              //carried by the IP datagram
                                                        ipHeader.MessageLength);//Length of the data field                    

                    return new[] { Tuple.Create("TCPv4", tcpHeader.DestinationPort), Tuple.Create("TCPv4", tcpHeader.SourcePort) };

                case Protocol.Udp:

                    var udpHeader = new UdpHeader(ipHeader.Data,              //IPHeader.Data stores the data being 
                                                                              //carried by the IP datagram
                                                       ipHeader.MessageLength);//Length of the data field                    

                    return new[] { Tuple.Create("UDPv4", udpHeader.DestinationPort), Tuple.Create("UDPv4", udpHeader.SourcePort) };

                case Protocol.Unknown:
                    return null;
                default:
                    return null;
            }
        }
    }
}
