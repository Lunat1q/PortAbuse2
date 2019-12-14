using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using PortAbuse2.Core.Ip;
using SharpPcap;
using SharpPcap.Npcap;

namespace PortAbuse2.Core.Listener
{
    public class CaptureDevicesInfo : IEnumerable<ICaptureDevice>
    {
        private readonly ICollection<ICaptureDevice> _captureDevices = new LinkedList<ICaptureDevice>();
        private readonly HashSet<PhysicalAddress> _physicalAddresses = new HashSet<PhysicalAddress>();
        private readonly HashSet<PhysicalAddress> _localAddresses = new HashSet<PhysicalAddress>();

        public CaptureDevicesInfo()
        {
            this.Init();
        }

        public void Clear()
        {
            this._captureDevices.Clear();
            this._physicalAddresses.Clear();
            this._localAddresses.Clear();
            this.Init();
        }

        private void Init()
        {
            this._localAddresses.Add(new PhysicalAddress(new byte[] {1, 0, 1, 0, 0, 0}));
        }

        public void Add(NpcapDevice device)
        {
            if (device.Interface.Addresses.Count > 0 && !Equals(device.Interface.MacAddress, PhysicalAddress.None))
            {
                this._physicalAddresses.Add(device.Interface.MacAddress);
            }

            this._captureDevices.Add(device);
        }

        public bool HavePhysicalAddressesInitialized()
        {
            return this._physicalAddresses.Count > 0;
        }

        public bool IsSelectedDeviceAddress(PhysicalAddress address)
        {
            var result = this._physicalAddresses.Contains(address);
            return result; // for debug purpose
        }

        public bool Any()
        {
            return this._captureDevices.Count > 0;
        }

        public IEnumerator<ICaptureDevice> GetEnumerator()
        {
            return this._captureDevices.GetEnumerator();
        }

        public bool IsLocalDeviceAddress(PhysicalAddress address)
        {
            var ret = this._localAddresses.Contains(address);
            return ret;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public void AddLocalPhysicalAddress(PhysicalAddress interfaceMacAddress)
        {
            this._localAddresses.Add(interfaceMacAddress);
        }

        public void SelectedCaptureDevice(IpInterface selectedIpInterface)
        {
            this.Clear();
            CaptureDeviceList deviceList = CaptureDeviceList.Instance;
            foreach (var device in deviceList.OfType<NpcapDevice>())
            {
                if (device.Name == selectedIpInterface?.HwName)
                {
                    this.Add(device);
                }

                if (device.Interface.Addresses.Count > 0) // just for the debugging purpose - save the local MacAddresses
                {
                    this.AddLocalPhysicalAddress(device.Interface.MacAddress);
                }
            }

            if (!this.Any()) //no device found, let's make an assumption and pick first
                this.Add(deviceList.OfType<NpcapDevice>().First()); // if we take all -> huge perf fckup
        }
    }
}