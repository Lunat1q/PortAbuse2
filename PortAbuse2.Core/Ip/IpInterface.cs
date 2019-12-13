using System;
using System.Collections.Generic;
using System.Linq;
using PortAbuse2.Core.Win32;
using SharpPcap;
using SharpPcap.LibPcap;
using SharpPcap.Npcap;

namespace PortAbuse2.Core.Ip
{
    public class IpInterface
    {
        private IpInterface(string hwName, string interfaceName)
        {
            this.HwName = hwName;
            this.FriendlyName = interfaceName;
        }

        public static IEnumerable<IpInterface> GetIpInterfaces()
        {
            var adapters = IpHlpApi.GetIPAdapters(IpHlpApi.FAMILY.AF_UNSPEC);
            var devices = CaptureDeviceList.Instance.OfType<NpcapDevice>();
            return devices.Where(x => !x.Loopback).Select(x => new IpInterface(x.Name, GetFriendlyName(x, adapters)));
        }

        private static string GetFriendlyName(LibPcapLiveDevice nPCapDevice, IEnumerable<IpHlpApi.IP_ADAPTER_ADDRESSES> adapters)
        {
            var friendlyName = nPCapDevice.Interface.FriendlyName;
            if (string.IsNullOrWhiteSpace(friendlyName))
            {
                var name = nPCapDevice.Name.Substring(@"\Device\NPF_".Length);
                var matchedAdapter = adapters.FirstOrDefault(x => x.AdapterName.Equals(name, StringComparison.OrdinalIgnoreCase));
                friendlyName = string.IsNullOrWhiteSpace(matchedAdapter.FriendlyName) ? nPCapDevice.Description : $"{matchedAdapter.FriendlyName} - {matchedAdapter.Description}";
            }
            else
            {
                friendlyName = $"{friendlyName} - {nPCapDevice.Description}";
            }

            var addresses = string.Join(", ",
                nPCapDevice.Addresses.Where(x => x.Addr.sa_family == 2).Select(x => x.Addr));
            addresses = string.IsNullOrWhiteSpace(addresses) ? string.Empty : $"({addresses})";
            return $"{friendlyName} {addresses}";
        }

        public string FriendlyName { get; set; }

        public string HwName { get; set; }
    }
}
