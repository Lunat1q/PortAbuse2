using System.Net;
using System.Net.NetworkInformation;

namespace PortAbuse2.Core.Proto.Tcp
{
    public class TcpEntry
    {

        #region "Main var"

        #endregion
        private string _mProcessName;
        #region "Main properties"
        public TcpState State { get; }

        public IPEndPoint RemoteEndPoint { get; }

        public IPEndPoint LocalEndPoint { get; }

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
        public TcpEntry(TcpState state, uint remoteAddr, int remotePort, uint localAddress, int localPort, int processId)
        {
            State = state;
            RemoteEndPoint = new IPEndPoint(remoteAddr, remotePort);
            ProcessId = processId;
            LocalEndPoint = new IPEndPoint(localAddress, localPort);
        }
    }
}
