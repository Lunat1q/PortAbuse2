using System;
using System.IO;
using System.Net;

namespace PortAbuse2.Core.Proto.Udp
{
    public class UdpHeader
    {
        //UDP header fields
        private readonly ushort _usSourcePort;            //Sixteen bits for the source port number        
        private readonly ushort _usDestinationPort;       //Sixteen bits for the destination port number
        private readonly ushort _usLength;                //Length of the UDP header
        private readonly short _sChecksum;                //Sixteen bits for the checksum
                                                //(checksum can be negative so taken as short)              
                                                //End UDP header fields

        public UdpHeader(byte[] byBuffer, int nReceived)
        {
            var memoryStream = new MemoryStream(byBuffer, 0, nReceived);
            var binaryReader = new BinaryReader(memoryStream);

            //The first sixteen bits contain the source port
            _usSourcePort = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

            //The next sixteen bits contain the destination port
            _usDestinationPort = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

            //The next sixteen bits contain the length of the UDP packet
            _usLength = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

            //The next sixteen bits contain the checksum
            _sChecksum = IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

            //Copy the data carried by the UDP packet into the data buffer
            try
            {
                Array.Copy(byBuffer,
                           8,               //The UDP header is of 8 bytes so we start copying after it
                           Data,
                           0,
                           nReceived - 8);
            }
            catch
            {
                // ignored
            }
        }

        public string SourcePort => _usSourcePort.ToString();

        public string DestinationPort => _usDestinationPort.ToString();

        public string Length => _usLength.ToString();

        public string Checksum => $"0x{_sChecksum:x2}";

        public byte[] Data { get; } = new byte[65536];
    }
}
