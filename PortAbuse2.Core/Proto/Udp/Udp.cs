using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace PortAbuse2.Core.Proto.Udp
{
    public class Udp
    {
        #region "var of UDP table"
        //Const using to know what kind of connection it is (IPv4 / IPv6)
        private const int AfInet = 2;
        //Const that allow you to sort TCP table
        private const bool BOrder = true;
        //Const for GetExtendedTcpTable and GetExtendedUdpTable

        private const int DwReserved = 0;
        //Structure for retrieving data
        private enum UdpTableClass
        {
            UdpTableBasic,
            UdpTableOwnerPid,
            UdpTableOwnerModule
        }

        //Structure for TCP table entries        
        private struct MibUdprowEx
        {
            public readonly uint DwLocalAddr;
            public readonly int DwLocalPort;
            public readonly int DwProcessId;

            public MibUdprowEx(int dwLocalPort, uint dwLocalAddr, int dwProcessId)
            {
                DwLocalPort = dwLocalPort;
                DwLocalAddr = dwLocalAddr;
                DwProcessId = dwProcessId;
            }
        }

        //List that will contains all data from each entries according to TcpEntry class
        private readonly List<UdpEntry> _mUdpTable = new List<UdpEntry>();
        public IEnumerable<UdpEntry> UdpTable => _mUdpTable;

        #endregion

        public Udp()
        {
            RetrieveUdpInfo();
        }

        [DllImport("iphlpapi.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        private static extern int GetExtendedUdpTable(IntPtr pUdpTable, ref int dwOutBufLen, bool bOrder, int dwFamily, UdpTableClass dwClass, int dwReserved);
        //Using of TCP table (from iphlpapi.dll)
        public void RetrieveUdpInfo()
        {
            //ptr of TCP table
            var ptrTcpTable = IntPtr.Zero;
            //size of TCP table
            var outBufferLenght = 0;
            //Return values with APIs
            //Number of table entries
            //ptr that define the number of data per entries

            //Var for retrieving data from entries


            //getting TCp table size
            var ret = GetExtendedUdpTable(ptrTcpTable, ref outBufferLenght, BOrder, AfInet, UdpTableClass.UdpTableOwnerPid, DwReserved);

            //Allocate memory to read and retrieve information from TCP table
            try
            {
                ptrTcpTable = Marshal.AllocHGlobal(outBufferLenght);
                ret = GetExtendedUdpTable(ptrTcpTable, ref outBufferLenght, BOrder, AfInet, UdpTableClass.UdpTableOwnerPid, DwReserved);
                if (ret == 0)
                {
                    //retrieving numbers of entries
                    var dwNumEntries = Marshal.ReadInt32(ptrTcpTable);
                    var ptr = new IntPtr(ptrTcpTable.ToInt64() + 4);

                    for (var i = 0; i <= dwNumEntries - 1; i++)
                    {
                        //allocate memory and reading entry i
                        var entry = (MibUdprowEx)Marshal.PtrToStructure(ptr, typeof(MibUdprowEx));
                        //retrieving data from entry i
                        _mUdpTable.Add(new UdpEntry(entry.DwLocalAddr, entry.DwLocalPort, entry.DwProcessId));
                        ptr = new IntPtr(ptr.ToInt64() + Marshal.SizeOf(typeof(MibUdprowEx)));
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
