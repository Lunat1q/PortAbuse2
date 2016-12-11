using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace PortAbuse2.Core.Proto.Tcp
{
    public class Tcp
    {

        #region "var of TCP table"
        //Const using to know what kind of connection it is (IPv4 / IPv6)
        private const int AfInet = 2;
        //Const that allow you to sort TCP table
        private const bool BOrder = true;
        //Const for GetExtendedTcpTable and GetExtendedUdpTable

        private const int DwReserved = 0;
        //Structure for retrieving data
        private enum TCP_TABLE_CLASS
        {
            TcpTableBasicListener,
            TcpTableBasicConnections,
            TcpTableBasicAll,
            TcpTableOwnerPidListener,
            TcpTableOwnerPidConnections,
            TcpTableOwnerPidAll,
            TcpTableOwnerModuleListener,
            TcpTableOwnerModuleConnections,
            TcpTableOwnerModuleAll
        }

        //Structure for TCP table entries
        private struct MibTcprowEx
        {
            public readonly TcpState DwState;
            public readonly uint DwLocalAddr;
            public readonly int DwLocalPort;
            public readonly uint DwRemoteAddr;
            public readonly int DwRemotePort;
            public readonly int DwProcessId;

            public MibTcprowEx(uint dwLocalAddr, TcpState dwState, int dwLocalPort, uint dwRemoteAddr, int dwRemotePort, int dwProcessId)
            {
                DwLocalAddr = dwLocalAddr;
                DwState = dwState;
                DwLocalPort = dwLocalPort;
                DwRemoteAddr = dwRemoteAddr;
                DwRemotePort = dwRemotePort;
                DwProcessId = dwProcessId;
            }
        }

        //List that will contains all data from each entries according to TcpEntry class
        private readonly List<TcpEntry> _mTcpTable = new List<TcpEntry>();
        public IEnumerable<TcpEntry> TcpTable => _mTcpTable;

        #endregion

        public Tcp()
        {
            RetrieveTcpInfo();
        }
        [DllImport("iphlpapi.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        private static extern int GetExtendedTcpTable(IntPtr pTcpTable, ref int dwOutBufLen, bool bOrder, int dwFamily, TCP_TABLE_CLASS dwClass, int dwReserved);
        //Using of TCP table (from iphlpapi.dll)
        public void RetrieveTcpInfo()
        {
            //ptr of TCP table
            var ptrTcpTable = IntPtr.Zero;
            //size of TCP table
            var outBufferLenght = 0;
            //Return values with APIs
            //Number of table entries
            //ptr that define the number of data per entries
            //Var for retrieving data from entries
            var entry = new MibTcprowEx();



            //getting TCp table size
            var ret = GetExtendedTcpTable(ptrTcpTable, ref outBufferLenght, BOrder, AfInet, TCP_TABLE_CLASS.TcpTableOwnerPidAll, DwReserved);
            var iStructSize = Marshal.SizeOf(entry);
            //Allocate memory to read and retrieve information from TCP table
            try
            {
                ptrTcpTable = Marshal.AllocHGlobal(outBufferLenght);
                ret = GetExtendedTcpTable(ptrTcpTable, ref outBufferLenght, BOrder, AfInet, TCP_TABLE_CLASS.TcpTableOwnerPidAll, DwReserved);
                if (ret == 0)
                {
                    //retrieving numbers of entries
                    var dwNumEntries = Marshal.ReadInt32(ptrTcpTable);
                    var ptr = new IntPtr(ptrTcpTable.ToInt64() + 4);

                    var entryArr = new MibTcprowEx[dwNumEntries];
                    for (var i = 0; i <= dwNumEntries - 1; i++)
                    {
                        //allocate memory and reading entry i
                        entryArr[i] = (MibTcprowEx)Marshal.PtrToStructure(ptr, typeof(MibTcprowEx));
                        ptr = new IntPtr(ptr.ToInt64() + iStructSize);
                    }
                    for (var i = 0; i <= dwNumEntries - 1; i++)
                    {
                        try
                        {
                            //retrieving data from entry i
                            _mTcpTable.Add(new TcpEntry(entryArr[i].DwState, entryArr[i].DwRemoteAddr, entryArr[i].DwRemotePort, entryArr[i].DwLocalAddr, entryArr[i].DwLocalPort, entryArr[i].DwProcessId));
                        }
                        catch (Exception)
                        {
                           //ignore
                        }
                    }
                }
                else
                {
                    throw new System.ComponentModel.Win32Exception(ret);
                }
            }
            catch
            {
                throw new System.ComponentModel.Win32Exception(ret);
            }
            finally
            {
                Marshal.FreeHGlobal(ptrTcpTable);
            }

        }
    }
}
