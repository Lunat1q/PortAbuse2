using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using PortAbuse2.Core.Proto;
using TiqUtils.Utils;
// ReSharper disable InconsistentNaming
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedMember.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable BuiltInTypeReferenceStyle
// ReSharper disable FieldCanBeMadeReadOnly.Local

namespace PortAbuse2.Core.Port
{
    internal static class SocketConnectionsReader
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
        public static IEnumerable<TcpProcessRecord> GetAllTcpConnections()
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
                var tableRowPtr = (IntPtr) ((long) tcpTableRecordsPtr +
                                            Marshal.SizeOf(tcpRecordsTable.dwNumEntries));

                var processes = new HashSet<int>(Process.GetProcesses().Select(x => x.Id));

                // Reading and parsing the TCP records one by one from the table and
                // storing them in a list of 'TcpProcessRecord' structure type objects.
                for (var row = 0; row < tcpRecordsTable.dwNumEntries; row++)
                {
                    var tcpRow =
                        (MIB_TCPROW_OWNER_PID) Marshal.PtrToStructure(tableRowPtr, typeof(MIB_TCPROW_OWNER_PID));
                    tcpTableRecords.Add(new TcpProcessRecord(
                        new IPAddress(tcpRow.localAddr),
                        new IPAddress(tcpRow.remoteAddr),
                        BitConverter.ToUInt16(new[]
                        {
                            tcpRow.localPort[1],
                            tcpRow.localPort[0]
                        }, 0),
                        BitConverter.ToUInt16(new[]
                        {
                            tcpRow.remotePort[1],
                            tcpRow.remotePort[0]
                        }, 0),
                        tcpRow.owningPid, tcpRow.state,
                        processes.Any(x => x == tcpRow.owningPid)));
                    tableRowPtr = (IntPtr) ((long) tableRowPtr + Marshal.SizeOf(tcpRow));
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

        public static IEnumerable<TcpProcessRecord> GetActiveTcpConnectionsForProcess(IEnumerable<int> pIds)
        {
            var bufferSize = 0;
            var tcpTableRecords = new List<TcpProcessRecord>();

            var result = GetExtendedTcpTable(IntPtr.Zero, ref bufferSize, true, AF_INET,
                TcpTableClass.TCP_TABLE_OWNER_PID_ALL);

            var tcpTableRecordsPtr = Marshal.AllocHGlobal(bufferSize);

            try
            {
                result = GetExtendedTcpTable(tcpTableRecordsPtr, ref bufferSize, true, AF_INET,
                    TcpTableClass.TCP_TABLE_OWNER_PID_ALL);

                if (result != 0 || pIds == null)
                    return new List<TcpProcessRecord>();

                var tcpRecordsTable = (MIB_TCPTABLE_OWNER_PID)
                    Marshal.PtrToStructure(tcpTableRecordsPtr,
                        typeof(MIB_TCPTABLE_OWNER_PID));
                var tableRowPtr = (IntPtr) ((long) tcpTableRecordsPtr + Marshal.SizeOf(tcpRecordsTable.dwNumEntries));

                var localPIds = new HashSet<int>(pIds);
                for (var row = 0; row < tcpRecordsTable.dwNumEntries; row++)
                {
                    var tcpRow =
                        (MIB_TCPROW_OWNER_PID) Marshal.PtrToStructure(tableRowPtr, typeof(MIB_TCPROW_OWNER_PID));
                    if (localPIds.Contains(tcpRow.owningPid))
                    {
                        tcpTableRecords.Add(
                            new TcpProcessRecord(
                                new IPAddress(tcpRow.localAddr),
                                new IPAddress(tcpRow.remoteAddr),
                                BitConverter.ToUInt16(new[]
                                {
                                    tcpRow.localPort[1],
                                    tcpRow.localPort[0]
                                }, 0),
                                BitConverter.ToUInt16(new[]
                                {
                                    tcpRow.remotePort[1],
                                    tcpRow.remotePort[0]
                                }, 0),
                                tcpRow.owningPid,
                                tcpRow.state,
                                true
                            )
                        );
                    }

                    tableRowPtr = (IntPtr) ((long) tableRowPtr + Marshal.SizeOf(tcpRow));
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

            return tcpTableRecords;
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
        public static IEnumerable<UdpProcessRecord> GetAllUdpConnections()
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
                var tableRowPtr = (IntPtr) ((long) udpTableRecordPtr +
                                            Marshal.SizeOf(udpRecordsTable.dwNumEntries));

                var processes = new HashSet<int>(Process.GetProcesses().Select(x => x.Id));

                // Reading and parsing the UDP records one by one from the table and
                // storing them in a list of 'UdpProcessRecord' structure type objects.
                for (var i = 0; i < udpRecordsTable.dwNumEntries; i++)
                {
                    var udpRow = (MIB_UDPROW_OWNER_PID)
                        Marshal.PtrToStructure(tableRowPtr, typeof(MIB_UDPROW_OWNER_PID));
                    udpTableRecords.Add(
                        new UdpProcessRecord(
                            udpRow.LocalAddress,
                            udpRow.LocalPort,
                            udpRow.PID,
                            processes.Any(x => x == udpRow.PID)));
                    tableRowPtr = (IntPtr) ((long) tableRowPtr + Marshal.SizeOf(udpRow));
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

        public static IEnumerable<UdpProcessRecord> GetActiveUdpConnectionsForProcess(IEnumerable<int> pIds)
        {
            var bufferSize = 0;
            var udpTableRecords = new List<UdpProcessRecord>();

            var result = GetExtendedUdpTable(IntPtr.Zero, ref bufferSize, true,
                AF_INET, UdpTableClass.UDP_TABLE_OWNER_PID);

            var udpTableRecordPtr = Marshal.AllocHGlobal(bufferSize);

            try
            {
                result = GetExtendedUdpTable(udpTableRecordPtr, ref bufferSize, true,
                    AF_INET, UdpTableClass.UDP_TABLE_OWNER_PID);

                if (result != 0 || pIds == null)
                    return new List<UdpProcessRecord>();

                var udpRecordsTable = (MIB_UDPTABLE_OWNER_PID)
                    Marshal.PtrToStructure(udpTableRecordPtr, typeof(MIB_UDPTABLE_OWNER_PID));
                var tableRowPtr = (IntPtr) ((long) udpTableRecordPtr +
                                            Marshal.SizeOf(udpRecordsTable.dwNumEntries));

                var localPIds = new HashSet<int>(pIds);
                for (var i = 0; i < udpRecordsTable.dwNumEntries; i++)
                {
                    var udpRow =
                        (MIB_UDPROW_OWNER_PID) Marshal.PtrToStructure(tableRowPtr, typeof(MIB_UDPROW_OWNER_PID));
                    if (localPIds.Contains(udpRow.PID))
                    {
                        udpTableRecords.Add(
                            new UdpProcessRecord(
                                udpRow.LocalAddress,
                                udpRow.LocalPort,
                                udpRow.PID,
                                true));
                    }

                    tableRowPtr = (IntPtr) ((long) tableRowPtr + Marshal.SizeOf(udpRow));
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

            return udpTableRecords;
        }


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



    ///// <summary>
    ///// The structure contains an entry from the User Datagram Protocol (UDP) listener
    ///// table for IPv4 on the local computer. The entry also includes the process ID
    ///// (PID) that issued the call to the bind function for the UDP endpoint.
    ///// </summary>
    //[StructLayout(LayoutKind.Sequential)]
    //public struct MIB_UDPROW_OWNER_PID
    //{
    //    public uint localAddr;
    //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    //    public byte[] localPort;
    //    public int owningPid;
    //}

    [StructLayout(LayoutKind.Sequential)]
    public struct MIB_UDPROW_OWNER_PID
    {
        // DWORD is System.UInt32 in C#
        private readonly uint localAdr;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        private readonly byte[] localPort;

        private readonly uint owningPid;

        public int PID => (int) this.owningPid;

        public IPAddress LocalAddress => new IPAddress(this.localAdr);

        public ushort LocalPort => BitConverter.ToUInt16(
            new [] {this.localPort[1], this.localPort[0] }, 0);
    }

    ///// <summary>
    ///// The structure contains the User Datagram Protocol (UDP) listener table for IPv4
    ///// on the local computer. The table also includes the process ID (PID) that issued
    ///// the call to the bind function for each UDP endpoint.
    ///// </summary>
    //[StructLayout(LayoutKind.Sequential)]
    //public struct MIB_UDPTABLE_OWNER_PID
    //{
    //    public uint dwNumEntries;
    //    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct,
    //        SizeConst = 1)]
    //    public UdpProcessRecord[] table;
    //}

    [StructLayout(LayoutKind.Sequential)]
    public struct MIB_UDPTABLE_OWNER_PID
    {
        public uint dwNumEntries;
        private readonly MIB_UDPROW_OWNER_PID table;
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
        public override Protocol Protocol => Protocol.Tcp;
        public TcpProcessRecord(IPAddress localIp, IPAddress remoteIp, ushort localPort,
            ushort remotePort, int pId, MibTcpState state, bool isValidProcess) : base(localIp, localPort, pId, isValidProcess)
        {
            this.RemoteAddress = remoteIp;
            this.RemotePort = remotePort;
            this.State = state;
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

        public abstract Protocol Protocol { get; }

        protected ProcessRecordBase(IPAddress localAddress, uint localPort, int pId, bool isValidProcess)
        {
            this.LocalAddress = localAddress;
            this.LocalPort = localPort;
            this.ProcessId = pId;

            if (!isValidProcess) return;

            var proc = Process.GetProcessById(this.ProcessId);
            this.ProcessName = proc.ProcessName;
            if (SystemProcess(this.ProcessName)) return;
            try
            {
                this.Title = proc.MainWindowTitle;
                this.FullName = proc.MainModule.FileName;
            }
            catch (Win32Exception)
            {
                //ignore
            }
        }

        private static bool SystemProcess(string name)
        {
            name = name.ToLowerInvariant();
            return name == "system" || name == "idle" || name == "wininit" || name == "services" || name == "msmpeng";
        }
    }

    /// <summary>
    /// This class provides access an IPv4 UDP connection addresses and ports and its
    /// associated Process IDs and names.
    /// </summary>
    public class UdpProcessRecord : ProcessRecordBase
    {
        public override Protocol Protocol => Protocol.Udp;
        public UdpProcessRecord(IPAddress localAddress, uint localPort, int pId, bool isValidProcess) : 
            base(localAddress, localPort, pId, isValidProcess)
        {
            
        }
    }
}
