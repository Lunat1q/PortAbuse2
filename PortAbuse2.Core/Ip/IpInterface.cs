using System;
using System.Collections.Generic;
using System.Linq;
using PortAbuse2.Core.Win32;
using SharpPcap;
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
            return devices.Select(x => new IpInterface(x.Name, x.Interface.FriendlyName ?? GetFriendlyName(x, adapters)));
        }

        private static string GetFriendlyName(ICaptureDevice nPCapDevice, IEnumerable<IpHlpApi.IP_ADAPTER_ADDRESSES> adapters)
        {
            var name = nPCapDevice.Name.Substring(@"\Device\NPF_".Length);
            var matchedAdapter = adapters.FirstOrDefault(x => x.AdapterName.Equals(name, StringComparison.OrdinalIgnoreCase));
            return string.IsNullOrWhiteSpace(matchedAdapter.FriendlyName) ? nPCapDevice.Description : matchedAdapter.FriendlyName;
        }

        public string FriendlyName { get; set; }

        public string HwName { get; set; }
    }
}
