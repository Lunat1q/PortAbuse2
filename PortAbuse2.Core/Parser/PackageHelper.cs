using System;
using System.Collections.Generic;
using PacketDotNet;
using PortAbuse2.Core.Proto;

namespace PortAbuse2.Core.Parser
{
    public static class PackageHelper
    {
        public static IEnumerable<Tuple<Protocol, ushort>> GetPorts(Packet packet, out IpPacket ipPacket, out TcpPacket tcpPacket, out UdpPacket udpPacket)
        {
            ipPacket = (IpPacket)packet.Extract(typeof(IpPacket));
            udpPacket = null;
            tcpPacket = null;
            if (ipPacket == null) return null;

            try
            {


                switch (ipPacket.Protocol)
                {
                    case IPProtocolType.TCP:
                        tcpPacket = (TcpPacket) packet.Extract(typeof(TcpPacket));
                        if (tcpPacket == null) return null;
                        return new[]
                        {
                            Tuple.Create(Protocol.Tcp, tcpPacket.DestinationPort),
                            Tuple.Create(Protocol.Tcp, tcpPacket.SourcePort)
                        };

                    case IPProtocolType.UDP:
                        udpPacket = (UdpPacket) packet.Extract(typeof(UdpPacket));
                        if (udpPacket == null) return null;
                        return new[]
                        {
                            Tuple.Create(Protocol.Udp, udpPacket.DestinationPort),
                            Tuple.Create(Protocol.Udp, udpPacket.SourcePort)
                        };
                    default:
                        return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
