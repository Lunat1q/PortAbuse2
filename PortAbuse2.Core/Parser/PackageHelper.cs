using System;
using PacketDotNet;
using PortAbuse2.Core.Proto;

namespace PortAbuse2.Core.Parser
{
    public static class PackageHelper
    {
        public static PortInformation GetPorts(Packet packet, out IPPacket ipPacket, out TcpPacket tcpPacket, out UdpPacket udpPacket)
        {
            ipPacket = packet.Extract<IPPacket>();
            udpPacket = null;
            tcpPacket = null;

            if (ipPacket == null) return new PortInformation();
            try
            {
                switch (ipPacket.Protocol)
                {
                    case ProtocolType.Tcp:
                        tcpPacket = packet.Extract<TcpPacket>();
                        return tcpPacket == null ? new PortInformation() : new PortInformation(Protocol.Tcp, tcpPacket.DestinationPort, tcpPacket.SourcePort);

                    case ProtocolType.Udp:
                        udpPacket = packet.Extract<UdpPacket>();
                        return udpPacket == null ? new PortInformation() : new PortInformation(Protocol.Udp, udpPacket.DestinationPort, udpPacket.SourcePort);
                    default:
                        return new PortInformation();
                }
            }
            catch (Exception)
            {
                return new PortInformation();
            }
        }
    }

    public struct PortInformation
    {
        public Protocol Protocol;

        public ushort DestinationPort;

        public ushort SourcePort;

        public PortInformation(Protocol protocol, ushort dPort, ushort sPort)
        {
            this.Protocol = protocol;
            this.DestinationPort = dPort;
            this.SourcePort = sPort;
        }

        public PortInformation(Protocol protocol = Protocol.Unknown) : this(protocol, 0, 0)
        {
        }
    }
}
