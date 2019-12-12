using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using PortAbuse2.Core.Win32;
using SharpPcap;
using SharpPcap.Npcap;
using SharpPcap.WinPcap;

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
            return devices.Select(x => new IpInterface(x.Name, x.Interface.FriendlyName ?? GetFriendlyName(x, adapters)));
        }

        private static string GetFriendlyName(NpcapDevice npcapDevice, IList<IpHlpApi.IP_ADAPTER_ADDRESSES> adapters)
        {
            var name = npcapDevice.Name.Substring(@"\Device\NPF_".Length);
            var matchedAdapter = adapters.Where(x => x.AdapterName.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (matchedAdapter.Any())
            {
                return matchedAdapter.First().FriendlyName;
            }

            return npcapDevice.Description;
        }

        public string FriendlyName { get; set; }

        public string HwName { get; set; }



    }
}
