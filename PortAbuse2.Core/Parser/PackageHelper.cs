using System;
using System.Collections.Generic;
using PacketDotNet;
using PortAbuse2.Core.Proto;

namespace PortAbuse2.Core.Parser
{
    public static class PackageHelper
    {
        public static IEnumerable<Tuple<Protocol, ushort>> GetPorts(Packet packet, out IPPacket ipPacket, out TcpPacket tcpPacket, out UdpPacket udpPacket)
        {
            ipPacket = packet.Extract<IPPacket>();
            udpPacket = null;
            tcpPacket = null;
            if (ipPacket == null) return null;

            try
            {


                switch (ipPacket.Protocol)
                {
                    case ProtocolType.Tcp:
                        tcpPacket = (TcpPacket) packet.Extract<TcpPacket>();
                        if (tcpPacket == null) return null;
                        return new[]
                        {
                            Tuple.Create(Protocol.Tcp, tcpPacket.DestinationPort),
                            Tuple.Create(Protocol.Tcp, tcpPacket.SourcePort)
                        };

                    case ProtocolType.Udp:
                        udpPacket = (UdpPacket) packet.Extract<UdpPacket>();
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
