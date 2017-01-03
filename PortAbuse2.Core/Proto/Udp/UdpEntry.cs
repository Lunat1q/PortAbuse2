using System;
using System.Net;

namespace PortAbuse2.Core.Proto.Udp
{
    public class UdpEntry
    {

        #region "Main var"
        //private UdpState m_State;

        #endregion
        private string _mProcessName;
        #region "Main properties"

        public IPEndPoint RemoteEndPoint { set; get; }

        public IPEndPoint LocalEndPoint { set; get; }

        public int ProcessId { get; }

        public string ProcessName
        {
            get
            {
                if (string.IsNullOrEmpty(_mProcessName))
                {
                    _mProcessName = System.Diagnostics.Process.GetProcessById(ProcessId).ProcessName;
                }
                return _mProcessName;
            }
        }
        #endregion
        public UdpEntry(UInt32 localAddress, int localPort, int processId)
        {
            //this.m_RemoteEndPoint = new IPEndPoint(remoteAddr, remotePort);
            ProcessId = processId;
            LocalEndPoint = new IPEndPoint(localAddress, localPort);
        }
    }
}
