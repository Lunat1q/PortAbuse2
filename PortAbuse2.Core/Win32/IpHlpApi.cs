using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PortAbuse2.Core.Win32
{
    internal static class IpHlpApi
    {
        public enum FAMILY : uint
        {
            /// <summary>
            /// IPv4
            /// </summary>
            AF_INET = 2,

            /// <summary>
            /// IPv6
            /// </summary>
            AF_INET6 = 23,

            /// <summary>
            /// Unpecified. Includes both IPv4 and IPv6
            /// </summary>
            AF_UNSPEC = 0
        }

        [Flags]
        public enum FLAGS : uint
        {
            GAA_FLAG_SKIP_UNICAST = 0x0001,
            GAA_FLAG_SKIP_ANYCAST = 0x0002,
            GAA_FLAG_SKIP_MULTICAST = 0x0004,
            GAA_FLAG_SKIP_DNS_SERVER = 0x0008,
            GAA_FLAG_INCLUDE_PREFIX = 0x0010,
            GAA_FLAG_SKIP_FRIENDLY_NAME = 0x0020,
            IncludeWins = 0x00000040,
            IncludeGateways = 0x00000080,
            IncludeAllInterfaces = 0x00000100,
            IncludeAllCompartments = 0x00000200,
        }

        public enum ERROR : uint
        {
            ERROR_SUCCESS = 0,
            ERROR_NO_DATA = 232,
            ERROR_BUFFER_OVERFLOW = 111,
            ERROR_INVALID_PARAMETER = 87
        }

        public enum IF_OPER_STATUS : uint
        {
            IfOperStatusUp = 1,
            IfOperStatusDown,
            IfOperStatusTesting,
            IfOperStatusUnknown,
            IfOperStatusDormant,
            IfOperStatusNotPresent,
            IfOperStatusLowerLayerDown,
        }

        public enum IP_SUFFIX_ORIGIN : uint
        {
            /// IpSuffixOriginOther -> 0
            IpSuffixOriginOther = 0,
            IpSuffixOriginManual,
            IpSuffixOriginWellKnown,
            IpSuffixOriginDhcp,
            IpSuffixOriginLinkLayerAddress,
            IpSuffixOriginRandom,
        }

        public enum IP_PREFIX_ORIGIN : uint
        {
            /// IpPrefixOriginOther -> 0
            IpPrefixOriginOther = 0,
            IpPrefixOriginManual,
            IpPrefixOriginWellKnown,
            IpPrefixOriginDhcp,
            IpPrefixOriginRouterAdvertisement,
        }

        public enum IP_DAD_STATE : uint
        {
            /// IpDadStateInvalid -> 0
            IpDadStateInvalid = 0,
            IpDadStateTentative,
            IpDadStateDuplicate,
            IpDadStateDeprecated,
            IpDadStatePreferred,
        }

        public enum NET_IF_CONNECTION_TYPE : uint
        {
            NET_IF_CONNECTION_DEDICATED = 1,
            NET_IF_CONNECTION_PASSIVE = 2,
            NET_IF_CONNECTION_DEMAND = 3,
            NET_IF_CONNECTION_MAXIMUM = 4
        }

        public enum TUNNEL_TYPE : uint
        {
            TUNNEL_TYPE_NONE = 0,
            TUNNEL_TYPE_OTHER = 1,
            TUNNEL_TYPE_DIRECT = 2,
            TUNNEL_TYPE_6TO4 = 11,
            TUNNEL_TYPE_ISATAP = 13,
            TUNNEL_TYPE_TEREDO = 14,
            TUNNEL_TYPE_IPHTTPS = 15
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct GUID
        {
            public int Data1;
            public short Data2;
            public short Data3;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] Data4;
        }

        private const int MAX_ADAPTER_ADDRESS_LENGTH = 8;
        private const int MAX_ADAPTER_NAME_LENGTH = 256;
        private const int MAX_DHCPV6_DUID_LENGTH = 130;

        [StructLayout(LayoutKind.Sequential)]
        public struct SOCKADDR
        {
            /// u_short->unsigned short
            public ushort sa_family;

            /// char[14]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
            public byte[] sa_data;
        }

        [Flags]
        internal enum AdapterFlags
        {
            DnsEnabled = 1,
            RegisterAdapterSuffix = 2,
            DhcpEnabled = 4,
            ReceiveOnly = 8,
            NoMulticast = 16,
            Ipv6OtherStatefulConfig = 32,
            NetBiosOverTcp = 64,
            IPv4Enabled = 128,
            IPv6Enabled = 256,
            IPv6ManagedAddressConfigurationSupported = 512,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SOCKET_ADDRESS
        {
            public IntPtr lpSockAddr;
            public int iSockaddrLength;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IP_ADAPTER_UNICAST_ADDRESS
        {
            public UInt64 Alignment;
            public IntPtr Next;
            public SOCKET_ADDRESS Address;
            public IP_PREFIX_ORIGIN PrefixOrigin;
            public IP_SUFFIX_ORIGIN SuffixOrigin;
            public IP_DAD_STATE DadState;
            public uint ValidLifetime;
            public uint PreferredLifetime;
            public uint LeaseLifetime;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IP_ADAPTER_ADDRESSES
        {
            public uint Length;
            public uint Index;

            public IntPtr Next;

            [MarshalAs(UnmanagedType.LPStr)] public string AdapterName;

            public IntPtr FirstUnicastAddress;
            public IntPtr FirstAnycastAddress;
            public IntPtr FirstMulticastAddress;
            public IntPtr FirstDnsServerAddress;
            [MarshalAs(UnmanagedType.LPWStr)] public string DnsSuffix;

            [System.Runtime.InteropServices.MarshalAs(UnmanagedType.LPWStr)]
            public string Description;

            [System.Runtime.InteropServices.MarshalAs(UnmanagedType.LPWStr)]
            public string FriendlyName;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_ADAPTER_ADDRESS_LENGTH)]
            public byte[] PhysicalAddress;

            public uint PhysicalAddressLength;
            public AdapterFlags Flags;
            public uint Mtu;
            public NetworkInterfaceType Type;
            public OperationalStatus OperStatus;
            public uint Ipv6Index;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public uint[] ZoneIndices;

            public IntPtr FirstPrefix;

            // Items added for Vista
            // May need to be removed on Windows versions below Vista to work properly (?)
            public UInt64 TrasmitLinkSpeed;
            public UInt64 ReceiveLinkSpeed;
            public IntPtr FirstWinsServerAddress;
            public IntPtr FirstGatewayAddress;
            public uint Ipv4Metric;
            public uint Ipv6Metric;
            public UInt64 Luid;
            public SOCKET_ADDRESS Dhcpv4Server;
            public uint CompartmentId;
            public Guid NetworkGuid;
            public NET_IF_CONNECTION_TYPE ConnectionType;
            public TUNNEL_TYPE TunnelType;
            public SOCKET_ADDRESS Dhcpv6Server;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_DHCPV6_DUID_LENGTH)]
            public byte[] Dhcpv6ClientDuid;

            public uint Dhcpv6ClientDuidLength;
            public uint Dhcpv6Iaid;

            public override string ToString()
            {
                return $"{this.FriendlyName} - {this.Description}";
            }
        }

        [DllImport("iphlpapi.dll")]
        private static extern ERROR GetAdaptersAddresses(uint Family, uint Flags, IntPtr Reserved,
            IntPtr pAdapterAddresses, ref uint pOutBufLen);

        public static string MarshalString(IntPtr text)
        {
            // !!!: This should only be used with IP_ADAPTER_ADDRESSES.AdapterName since it can't be marshalled automatically
            byte[] cName = new byte[MAX_ADAPTER_NAME_LENGTH];
            Marshal.Copy(text, cName, 0, MAX_ADAPTER_NAME_LENGTH);
            string name = Encoding.ASCII.GetString(cName, 0, MAX_ADAPTER_NAME_LENGTH);

            return name;
        }

        public static IList<IP_ADAPTER_ADDRESSES> GetIPAdapters(FAMILY family)
        {
            List<IP_ADAPTER_ADDRESSES> adapters = new List<IP_ADAPTER_ADDRESSES>();
            uint outBufLen = 0;
            ERROR err = GetAdaptersAddresses(
                (uint) family,
                (uint) (FLAGS.IncludeAllInterfaces | FLAGS.IncludeGateways | FLAGS.IncludeWins | FLAGS.GAA_FLAG_INCLUDE_PREFIX), 
                IntPtr.Zero,
                IntPtr.Zero, ref outBufLen);

            if (ERROR.ERROR_BUFFER_OVERFLOW == err)
            {
                IntPtr pAdapterAddresses = Marshal.AllocHGlobal((int) outBufLen);
                try
                {
                    err = GetAdaptersAddresses((uint) FAMILY.AF_INET, (uint) FLAGS.IncludeAllInterfaces, IntPtr.Zero,
                        pAdapterAddresses, ref outBufLen);
                    if (ERROR.ERROR_SUCCESS == err)
                    {
                        IntPtr currPtr = pAdapterAddresses;
                        while (IntPtr.Zero != currPtr)
                        {
                            IP_ADAPTER_ADDRESSES addr =
                                (IP_ADAPTER_ADDRESSES) Marshal.PtrToStructure(currPtr, typeof(IP_ADAPTER_ADDRESSES));
                            adapters.Add(addr);

                            currPtr = addr.Next;
                        }
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(pAdapterAddresses);
                }
            }

            return adapters;
        }

        public static IList<IPAddress> GetIPAddresses(FAMILY family)
        {
            List<IPAddress> addresses = new List<IPAddress>();

            foreach (IP_ADAPTER_ADDRESSES addr in GetIPAdapters(family))
            {
                if (IntPtr.Zero != addr.FirstUnicastAddress)
                {
                    IP_ADAPTER_UNICAST_ADDRESS unicastAddr =
                        (IP_ADAPTER_UNICAST_ADDRESS) Marshal.PtrToStructure(addr.FirstUnicastAddress,
                            typeof(IP_ADAPTER_UNICAST_ADDRESS));
                    if (IntPtr.Zero != unicastAddr.Address.lpSockAddr)
                    {
                        SOCKADDR socketAddr =
                            (SOCKADDR) Marshal.PtrToStructure(unicastAddr.Address.lpSockAddr, typeof(SOCKADDR));
                        byte[] saData = socketAddr.sa_data.Skip(2).Take(4).ToArray();
                        IPAddress ipAddr = new IPAddress(saData);
                        addresses.Add(ipAddr);
                    }
                }
            }

            return addresses;
        }
    }
}
