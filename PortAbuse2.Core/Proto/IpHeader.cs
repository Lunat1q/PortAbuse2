using System;
using System.IO;
using System.Net;

namespace PortAbuse2.Core.Proto
{
    public class IpHeader
    {
        //IP Header fields
        private readonly byte _byVersionAndHeaderLength;   //Eight bits for version and header length
        private readonly byte _byDifferentiatedServices;    //Eight bits for differentiated services (TOS)
        private readonly ushort _usTotalLength;              //Sixteen bits for total length of the datagram (header + message)
        private readonly ushort _usIdentification;           //Sixteen bits for identification
        private readonly ushort _usFlagsAndOffset;           //Eight bits for flags and fragmentation offset
        private readonly byte _byTtl;                      //Eight bits for TTL (Time To Live)
        private readonly byte _byProtocol;                 //Eight bits for the underlying protocol
        private readonly short _sChecksum;                  //Sixteen bits containing the checksum of the header
                                                  //(checksum can be negative so taken as short)
        private readonly uint _uiSourceIpAddress;          //Thirty two bit source IP Address
        private readonly uint _uiDestinationIpAddress;     //Thirty two bit destination IP Address
                                                 //End IP Header fields

        private readonly byte _byHeaderLength;             //Header length
        private readonly byte[] _byIpData = new byte[65536];  //Data carried by the datagram


        public IpHeader(byte[] byBuffer, int nReceived)
        {

            //try
            //{
            //Create MemoryStream out of the received bytes
            MemoryStream memoryStream = new MemoryStream(byBuffer, 0, nReceived);
            //Next we create a BinaryReader out of the MemoryStream
            BinaryReader binaryReader = new BinaryReader(memoryStream);

            //The first eight bits of the IP header contain the version and
            //header length so we read them
            _byVersionAndHeaderLength = binaryReader.ReadByte();

            //The next eight bits contain the Differentiated services
            _byDifferentiatedServices = binaryReader.ReadByte();

            //Next eight bits hold the total length of the datagram
            _usTotalLength = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

            //Next sixteen have the identification bytes
            _usIdentification = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

            //Next sixteen bits contain the flags and fragmentation offset
            _usFlagsAndOffset = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

            //Next eight bits have the TTL value
            _byTtl = binaryReader.ReadByte();

            //Next eight represnts the protocol encapsulated in the datagram
            _byProtocol = binaryReader.ReadByte();

            //Next sixteen bits contain the checksum of the header
            _sChecksum = IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

            //Next thirty two bits have the source IP address
            _uiSourceIpAddress = (uint)(binaryReader.ReadInt32());

            //Next thirty two hold the destination IP address
            _uiDestinationIpAddress = (uint)(binaryReader.ReadInt32());

            //Now we calculate the header length

            _byHeaderLength = _byVersionAndHeaderLength;
            //The last four bits of the version and header length field contain the
            //header length, we perform some simple binary airthmatic operations to
            //extract them
            _byHeaderLength <<= 4;
            _byHeaderLength >>= 4;
            //Multiply by four to get the exact header length
            _byHeaderLength *= 4;

            //Copy the data carried by the data gram into another array so that
            //according to the protocol being carried in the IP datagram
            //try
            //{
            Array.Copy(byBuffer,
                       _byHeaderLength,  //start copying from the end of the header
                       _byIpData, 0,
                       _usTotalLength - _byHeaderLength);
            //    }
            //    catch
            //    {
            //        //if (byHeaderLength > 
            //    }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message, "IP Header - Error", MessageBoxButtons.OK,
            //        MessageBoxIcon.Error);
            //}
        }

        public string Version
        {
            get
            {
                //Calculate the IP version

                //The four bits of the IP header contain the IP version
                if ((_byVersionAndHeaderLength >> 4) == 4)
                {
                    return "IP v4";
                }
                else if ((_byVersionAndHeaderLength >> 4) == 6)
                {
                    return "IP v6";
                }
                else
                {
                    return "Unknown";
                }
            }
        }

        public string HeaderLength => _byHeaderLength.ToString();

        public ushort MessageLength => (ushort)(_usTotalLength - _byHeaderLength);

        public string DifferentiatedServices => $"0x{_byDifferentiatedServices:x2} ({_byDifferentiatedServices})";

        public string Flags
        {
            get
            {
                //The first three bits of the flags and fragmentation field 
                //represent the flags (which indicate whether the data is 
                //fragmented or not)
                int nFlags = _usFlagsAndOffset >> 13;
                if (nFlags == 2)
                {
                    return "Don't fragment";
                }
                else if (nFlags == 1)
                {
                    return "More fragments to come";
                }
                else
                {
                    return nFlags.ToString();
                }
            }
        }

        public string FragmentationOffset
        {
            get
            {
                //The last thirteen bits of the flags and fragmentation field 
                //contain the fragmentation offset
                int nOffset = _usFlagsAndOffset << 3;
                nOffset >>= 3;

                return nOffset.ToString();
            }
        }

        public string Ttl => _byTtl.ToString();

        public Protocol ProtocolType
        {
            get
            {
                //The protocol field represents the protocol in the data portion
                //of the datagram
                if (_byProtocol == 6)        //A value of six represents the TCP protocol
                {
                    return Protocol.Tcp;
                }
                else if (_byProtocol == 17)  //Seventeen for UDP
                {
                    return Protocol.Udp;
                }
                else
                {
                    return Protocol.Unknown;
                }
            }
        }

        public string Checksum => $"0x{_sChecksum:x2}";

        public IPAddress SourceAddress => new IPAddress(_uiSourceIpAddress);

        public IPAddress DestinationAddress => new IPAddress(_uiDestinationIpAddress);

        public string TotalLength => _usTotalLength.ToString();

        public string Identification => _usIdentification.ToString();

        public byte[] Data => _byIpData;
    }
}
