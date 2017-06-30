using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using TiqUtils.Utils;
// ReSharper disable InconsistentNaming
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedMember.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace PortAbuse2.Core.Port
{
    static class SocketConnectionsReader
    {
        // The version of IP used by the TCP/UDP endpoint. AF_INET is used for IPv4.
        private const int AF_INET = 2;

        // The GetExtendedTcpTable function retrieves a table that contains a list of
        // TCP endpoints available to the application. Decorating the function with
        // DllImport attribute indicates that the attributed method is exposed by an
        // unmanaged dynamic-link library 'iphlpapi.dll' as a static entry point.
        [DllImport("iphlpapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern uint GetExtendedTcpTable(IntPtr pTcpTable, ref int pdwSize,
            bool bOrder, int ulAf, TcpTableClass tableClass, uint reserved = 0);

        // The GetExtendedUdpTable function retrieves a table that contains a list of
        // UDP endpoints available to the application. Decorating the function with
        // DllImport attribute indicates that the attributed method is exposed by an
        // unmanaged dynamic-link library 'iphlpapi.dll' as a static entry point.
        [DllImport("iphlpapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern uint GetExtendedUdpTable(IntPtr pUdpTable, ref int pdwSize,
            bool bOrder, int ulAf, UdpTableClass tableClass, uint reserved = 0);

        /// <summary>
        /// This function reads and parses the active TCP socket connections available
        /// and stores them in a list.
        /// </summary>
        /// <returns>
        /// It returns the current set of TCP socket connections which are active.
        /// </returns>
        /// <exception cref="OutOfMemoryException">
        /// This exception may be thrown by the function Marshal.AllocHGlobal when there
        /// is insufficient memory to satisfy the request.
        /// </exception>
        public static List<TcpProcessRecord> GetAllTcpConnections()
        {
            var bufferSize = 0;
            var tcpTableRecords = new List<TcpProcessRecord>();

            // Getting the size of TCP table, that is returned in 'bufferSize' variable.
            var result = GetExtendedTcpTable(IntPtr.Zero, ref bufferSize, true, AF_INET,
                TcpTableClass.TCP_TABLE_OWNER_PID_ALL);

            // Allocating memory from the unmanaged memory of the process by using the
            // specified number of bytes in 'bufferSize' variable.
            var tcpTableRecordsPtr = Marshal.AllocHGlobal(bufferSize);

            try
            {
                // The size of the table returned in 'bufferSize' variable in previous
                // call must be used in this subsequent call to 'GetExtendedTcpTable'
                // function in order to successfully retrieve the table.
                result = GetExtendedTcpTable(tcpTableRecordsPtr, ref bufferSize, true,
                    AF_INET, TcpTableClass.TCP_TABLE_OWNER_PID_ALL);

                // Non-zero value represent the function 'GetExtendedTcpTable' failed,
                // hence empty list is returned to the caller function.
                if (result != 0)
                    return new List<TcpProcessRecord>();

                // Marshals data from an unmanaged block of memory to a newly allocated
                // managed object 'tcpRecordsTable' of type 'MIB_TCPTABLE_OWNER_PID'
                // to get number of entries of the specified TCP table structure.
                var tcpRecordsTable = (MIB_TCPTABLE_OWNER_PID)
                    Marshal.PtrToStructure(tcpTableRecordsPtr,
                        typeof(MIB_TCPTABLE_OWNER_PID));
                var tableRowPtr = (IntPtr)((long)tcpTableRecordsPtr +
                                              Marshal.SizeOf(tcpRecordsTable.dwNumEntries));

                var processes = Process.GetProcesses();

                // Reading and parsing the TCP records one by one from the table and
                // storing them in a list of 'TcpProcessRecord' structure type objects.
                for (var row = 0; row < tcpRecordsTable.dwNumEntries; row++)
                {
                    var tcpRow = (MIB_TCPROW_OWNER_PID)Marshal.
                        PtrToStructure(tableRowPtr, typeof(MIB_TCPROW_OWNER_PID));
                    tcpTableRecords.Add(new TcpProcessRecord(
                        new IPAddress(tcpRow.localAddr),
                        new IPAddress(tcpRow.remoteAddr),
                        BitConverter.ToUInt16(new[] {
                            tcpRow.localPort[1],
                            tcpRow.localPort[0] }, 0),
                        BitConverter.ToUInt16(new[] {
                            tcpRow.remotePort[1],
                            tcpRow.remotePort[0] }, 0),
                        tcpRow.owningPid, tcpRow.state,
                        processes));
                    tableRowPtr = (IntPtr)((long)tableRowPtr + Marshal.SizeOf(tcpRow));
                }
            }
            catch (OutOfMemoryException outOfMemoryException)
            {
                Logging.ErrorLog("Out Of Memory: " + outOfMemoryException.Message);
            }
            catch (Exception exception)
            {
                Logging.ErrorLog("Exception: " + exception.Message);
            }
            finally
            {
                Marshal.FreeHGlobal(tcpTableRecordsPtr);
            }
            return tcpTableRecords.Distinct()
                .ToList();
        }

        /// <summary>
        /// This function reads and parses the active UDP socket connections available
        /// and stores them in a list.
        /// </summary>
        /// <returns>
        /// It returns the current set of UDP socket connections which are active.
        /// </returns>
        /// <exception cref="OutOfMemoryException">
        /// This exception may be thrown by the function Marshal.AllocHGlobal when there
        /// is insufficient memory to satisfy the request.
        /// </exception>
        public static List<UdpProcessRecord> GetAllUdpConnections()
        {
            var bufferSize = 0;
            var udpTableRecords = new List<UdpProcessRecord>();
            
            // Getting the size of UDP table, that is returned in 'bufferSize' variable.
            var result = GetExtendedUdpTable(IntPtr.Zero, ref bufferSize, true,
                AF_INET, UdpTableClass.UDP_TABLE_OWNER_PID);

            // Allocating memory from the unmanaged memory of the process by using the
            // specified number of bytes in 'bufferSize' variable.
            var udpTableRecordPtr = Marshal.AllocHGlobal(bufferSize);

            try
            {
                // The size of the table returned in 'bufferSize' variable in previous
                // call must be used in this subsequent call to 'GetExtendedUdpTable'
                // function in order to successfully retrieve the table.
                result = GetExtendedUdpTable(udpTableRecordPtr, ref bufferSize, true,
                    AF_INET, UdpTableClass.UDP_TABLE_OWNER_PID);

                // Non-zero value represent the function 'GetExtendedUdpTable' failed,
                // hence empty list is returned to the caller function.
                if (result != 0)
                    return new List<UdpProcessRecord>();

                // Marshals data from an unmanaged block of memory to a newly allocated
                // managed object 'udpRecordsTable' of type 'MIB_UDPTABLE_OWNER_PID'
                // to get number of entries of the specified TCP table structure.
                var udpRecordsTable = (MIB_UDPTABLE_OWNER_PID)
                    Marshal.PtrToStructure(udpTableRecordPtr, typeof(MIB_UDPTABLE_OWNER_PID));
                var tableRowPtr = (IntPtr)((long)udpTableRecordPtr +
                                              Marshal.SizeOf(udpRecordsTable.dwNumEntries));

                var processes = Process.GetProcesses();

                // Reading and parsing the UDP records one by one from the table and
                // storing them in a list of 'UdpProcessRecord' structure type objects.
                for (var i = 0; i < udpRecordsTable.dwNumEntries; i++)
                {
                    var udpRow = (MIB_UDPROW_OWNER_PID)
                        Marshal.PtrToStructure(tableRowPtr, typeof(MIB_UDPROW_OWNER_PID));
                    udpTableRecords.Add(new UdpProcessRecord(new IPAddress(udpRow.localAddr),
                        BitConverter.ToUInt16(new [] { udpRow.localPort[1],
                            udpRow.localPort[0] }, 0), udpRow.owningPid, processes));
                    tableRowPtr = (IntPtr)((long)tableRowPtr + Marshal.SizeOf(udpRow));
                }
            }
            catch (OutOfMemoryException outOfMemoryException)
            {
                Logging.ErrorLog("Out Of Memory: " + outOfMemoryException.Message);
            }
            catch (Exception exception)
            {
                Logging.ErrorLog("Exception: " + exception.Message);
            }
            finally
            {
                Marshal.FreeHGlobal(udpTableRecordPtr);
            }
            return udpTableRecords.Distinct()
                .ToList();
        }


    }

    // Enum for protocol types.
    public enum Protocol
    {
        TCP,
        UDP
    }

    // Enum to define the set of values used to indicate the type of table returned by 
    // calls made to the function 'GetExtendedTcpTable'.
    public enum TcpTableClass
    {
        TCP_TABLE_BASIC_LISTENER,
        TCP_TABLE_BASIC_CONNECTIONS,
        TCP_TABLE_BASIC_ALL,
        TCP_TABLE_OWNER_PID_LISTENER,
        TCP_TABLE_OWNER_PID_CONNECTIONS,
        TCP_TABLE_OWNER_PID_ALL,
        TCP_TABLE_OWNER_MODULE_LISTENER,
        TCP_TABLE_OWNER_MODULE_CONNECTIONS,
        TCP_TABLE_OWNER_MODULE_ALL
    }

    // Enum to define the set of values used to indicate the type of table returned by calls
    // made to the function GetExtendedUdpTable.
    public enum UdpTableClass
    {
        UDP_TABLE_BASIC,
        UDP_TABLE_OWNER_PID,
        UDP_TABLE_OWNER_MODULE
    }

    // Enum for different possible states of TCP connection
    public enum MibTcpState
    {
        CLOSED = 1,
        LISTENING = 2,
        SYN_SENT = 3,
        SYN_RCVD = 4,
        ESTABLISHED = 5,
        FIN_WAIT1 = 6,
        FIN_WAIT2 = 7,
        CLOSE_WAIT = 8,
        CLOSING = 9,
        LAST_ACK = 10,
        TIME_WAIT = 11,
        DELETE_TCB = 12,
        NONE = 0
    }

    /// <summary>
    /// The structure contains information that describes an IPv4 TCP connection with 
    /// IPv4 addresses, ports used by the TCP connection, and the specific process ID
    /// (PID) associated with connection.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MIB_TCPROW_OWNER_PID
    {
        public MibTcpState state;
        public uint localAddr;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] localPort;
        public uint remoteAddr;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] remotePort;
        public int owningPid;
    }

    /// <summary>
    /// The structure contains a table of process IDs (PIDs) and the IPv4 TCP links that 
    /// are context bound to these PIDs.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MIB_TCPTABLE_OWNER_PID
    {
        public uint dwNumEntries;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct,
            SizeConst = 1)]
        public MIB_TCPROW_OWNER_PID[] table;
    }

    

    /// <summary>
    /// The structure contains an entry from the User Datagram Protocol (UDP) listener
    /// table for IPv4 on the local computer. The entry also includes the process ID
    /// (PID) that issued the call to the bind function for the UDP endpoint.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MIB_UDPROW_OWNER_PID
    {
        public uint localAddr;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] localPort;
        public int owningPid;
    }

    /// <summary>
    /// The structure contains the User Datagram Protocol (UDP) listener table for IPv4
    /// on the local computer. The table also includes the process ID (PID) that issued
    /// the call to the bind function for each UDP endpoint.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MIB_UDPTABLE_OWNER_PID
    {
        public uint dwNumEntries;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct,
            SizeConst = 1)]
        public UdpProcessRecord[] table;
    }

    /// <summary>
    /// This class provides access an IPv4 TCP connection addresses and ports and its
    /// associated Process IDs and names.
    /// </summary>
    public class TcpProcessRecord : ProcessRecordBase
    {
        public IPAddress RemoteAddress { get; }
        public ushort RemotePort { get; }
        public MibTcpState State { get; }

        public TcpProcessRecord(IPAddress localIp, IPAddress remoteIp, ushort localPort,
            ushort remotePort, int pId, MibTcpState state, Process[] processList) : base(localIp, localPort, pId, processList)
        {
            RemoteAddress = remoteIp;
            RemotePort = remotePort;
            State = state;
        }
    }

    public abstract class ProcessRecordBase
    {
        public IPAddress LocalAddress { get; }
        public uint LocalPort { get; }
        public int ProcessId { get; }
        public string ProcessName { get; }
        public string Title { get; }
        public string FullName { get; }

        protected ProcessRecordBase(IPAddress localAddress, uint localPort, int pId, IEnumerable<Process> processList)
        {
            LocalAddress = localAddress;
            LocalPort = localPort;
            ProcessId = pId;
            if (processList.All(process => process.Id != pId)) return;
            var proc = Process.GetProcessById(ProcessId);
            ProcessName = proc.ProcessName;
            try
            {
                Title = proc.MainWindowTitle;
                FullName = proc.MainModule.FileName;
            }
            catch (Win32Exception)
            {
                //ignore
            }
        }
    }

    /// <summary>
    /// This class provides access an IPv4 UDP connection addresses and ports and its
    /// associated Process IDs and names.
    /// </summary>
    public class UdpProcessRecord : ProcessRecordBase
    {
        public UdpProcessRecord(IPAddress localAddress, uint localPort, int pId, IEnumerable<Process> processList) : 
            base(localAddress, localPort, pId, processList)
        {
            
        }
    }
}
