using System;
using System.IO;
using System.Net;

namespace PortAbuse2.Core.Proto.Tcp
{
    public class TcpHeader
    {
        //TCP header fields
        private readonly ushort _usSourcePort;              //Sixteen bits for the source port number
        private readonly ushort _usDestinationPort;         //Sixteen bits for the destination port number
        private readonly uint _uiSequenceNumber = 555;          //Thirty two bits for the sequence number
        private readonly uint _uiAcknowledgementNumber = 555;   //Thirty two bits for the acknowledgement number
        private readonly ushort _usDataOffsetAndFlags = 555;      //Sixteen bits for flags and data offset
        private readonly ushort _usWindow = 555;                  //Sixteen bits for the window size
        private readonly short _sChecksum = 555;                 //Sixteen bits for the checksum
                                                       //(checksum can be negative so taken as short)
        private readonly ushort _usUrgentPointer;           //Sixteen bits for the urgent pointer
        //End TCP header fields

        private readonly byte _byHeaderLength;            //Header length

        public TcpHeader(byte[] byBuffer, int nReceived)
        {
            try
            {
                MemoryStream memoryStream;
                BinaryReader binaryReader;
                try
                {
                    memoryStream = new MemoryStream(byBuffer, 0, nReceived);
                    binaryReader = new BinaryReader(memoryStream);
                }
                catch
                {
                    var nReceivedFixed = byBuffer.Length;
                    memoryStream = new MemoryStream(byBuffer, 0, nReceivedFixed);
                    binaryReader = new BinaryReader(memoryStream);
                }

                //The first sixteen bits contain the source port
                _usSourcePort = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

                //The next sixteen contain the destiination port
                _usDestinationPort = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

                //Next thirty two have the sequence number
                _uiSequenceNumber = (uint)IPAddress.NetworkToHostOrder(binaryReader.ReadInt32());

                //Next thirty two have the acknowledgement number
                _uiAcknowledgementNumber = (uint)IPAddress.NetworkToHostOrder(binaryReader.ReadInt32());

                //The next sixteen bits hold the flags and the data offset
                _usDataOffsetAndFlags = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

                //The next sixteen contain the window size
                _usWindow = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

                //In the next sixteen we have the checksum
                _sChecksum = IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

                //The following sixteen contain the urgent pointer
                _usUrgentPointer = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

                //The data offset indicates where the data begins, so using it we
                //calculate the header length
                _byHeaderLength = (byte)(_usDataOffsetAndFlags >> 12);
                _byHeaderLength *= 4;

                //Message length = Total length of the TCP packet - Header length
                MessageLength = (ushort)(nReceived - _byHeaderLength);

                //Copy the TCP data into the data buffer
                try
                {
                    Array.Copy(byBuffer, _byHeaderLength, Data, 0, nReceived - _byHeaderLength);
                }
                catch
                {
                    //ignore
                }
            }
            catch (Exception)
            {
                //ignore 
                //MessageBox.Show(ex.Message, "PortAbuse - TCP Header" + (nReceived), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public string SourcePort => _usSourcePort.ToString();

        public string DestinationPort => _usDestinationPort.ToString();

        public string SequenceNumber => _uiSequenceNumber.ToString();

        public string AcknowledgementNumber
        {
            get
            {
                //If the ACK flag is set then only we have a valid value in
                //the acknowlegement field, so check for it beore returning 
                //anything
                if ((_usDataOffsetAndFlags & 0x10) != 0)
                {
                    return _uiAcknowledgementNumber.ToString();
                }
                else
                    return "";
            }
        }

        public string HeaderLength => _byHeaderLength.ToString();

        public string WindowSize => _usWindow.ToString();

        public string UrgentPointer
        {
            get
            {
                //If the URG flag is set then only we have a valid value in
                //the urgent pointer field, so check for it beore returning 
                //anything
                if ((_usDataOffsetAndFlags & 0x20) != 0)
                {
                    return _usUrgentPointer.ToString();
                }
                else
                    return "";
            }
        }

        public string Flags
        {
            get
            {
                //The last six bits of the data offset and flags contain the
                //control bits

                //First we extract the flags
                var nFlags = _usDataOffsetAndFlags & 0x3F;

                string strFlags = $"0x{nFlags:x2} (";

                //Now we start looking whether individual bits are set or not
                if ((nFlags & 0x01) != 0)
                {
                    strFlags += "FIN, ";
                }
                if ((nFlags & 0x02) != 0)
                {
                    strFlags += "SYN, ";
                }
                if ((nFlags & 0x04) != 0)
                {
                    strFlags += "RST, ";
                }
                if ((nFlags & 0x08) != 0)
                {
                    strFlags += "PSH, ";
                }
                if ((nFlags & 0x10) != 0)
                {
                    strFlags += "ACK, ";
                }
                if ((nFlags & 0x20) != 0)
                {
                    strFlags += "URG";
                }
                strFlags += ")";

                if (strFlags.Contains("()"))
                {
                    strFlags = strFlags.Remove(strFlags.Length - 3);
                }
                else if (strFlags.Contains(", )"))
                {
                    strFlags = strFlags.Remove(strFlags.Length - 3, 2);
                }

                return strFlags;
            }
        }

        public string Checksum => $"0x{_sChecksum:x2}";

        public byte[] Data { get; } = new byte[65536];

        public ushort MessageLength { get; }
    }
}
