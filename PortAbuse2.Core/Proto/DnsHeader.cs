using System.IO;
using System.Net;

namespace PortAbuse2.Core.Proto
{
    public class DnsHeader
    {
        //DNS header fields
        private readonly ushort _usIdentification;        //Sixteen bits for identification
        private readonly ushort _usFlags;                 //Sixteen bits for DNS flags
        private readonly ushort _usTotalQuestions;        //Sixteen bits indicating the number of entries 
                                                //in the questions list
        private readonly ushort _usTotalAnswerRRs;        //Sixteen bits indicating the number of entries
                                                //entries in the answer resource record list
        private readonly ushort _usTotalAuthorityRRs;     //Sixteen bits indicating the number of entries
                                                //entries in the authority resource record list
        private readonly ushort _usTotalAdditionalRRs;    //Sixteen bits indicating the number of entries
                                                //entries in the additional resource record list
                                                //End DNS header fields

        public DnsHeader(byte[] byBuffer, int nReceived)
        {
            var memoryStream = new MemoryStream(byBuffer, 0, nReceived);
            var binaryReader = new BinaryReader(memoryStream);

            //First sixteen bits are for identification
            _usIdentification = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

            //Next sixteen contain the flags
            _usFlags = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

            //Read the total numbers of questions in the quesion list
            _usTotalQuestions = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

            //Read the total number of answers in the answer list
            _usTotalAnswerRRs = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

            //Read the total number of entries in the authority list
            _usTotalAuthorityRRs = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

            //Total number of entries in the additional resource record list
            _usTotalAdditionalRRs = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
        }

        public string Identification => $"0x{_usIdentification:x2}";

        public string Flags => $"0x{_usFlags:x2}";

        public string TotalQuestions => _usTotalQuestions.ToString();

        public string TotalAnswerRRs => _usTotalAnswerRRs.ToString();

        public string TotalAuthorityRRs => _usTotalAuthorityRRs.ToString();

        public string TotalAdditionalRRs => _usTotalAdditionalRRs.ToString();
    }
}
