using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using PortAbuse2.Core.Ip;
using SharpPcap;
using SharpPcap.LibPcap;

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
        }

        public void Add(LibPcapLiveDevice device)
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

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public void AddLocalPhysicalAddress(PhysicalAddress interfaceMacAddress)
        {
            this._localAddresses.Add(interfaceMacAddress);
        }

        public void SelectedCaptureDevice(IpInterface? selectedIpInterface)
        {
            this.Clear();
            CaptureDeviceList deviceList = CaptureDeviceList.Instance;
            foreach (var device in deviceList.OfType<LibPcapLiveDevice>())
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
                this.Add(deviceList.OfType<LibPcapLiveDevice>().First()); // if we take all -> huge perf fckup
        }

        public bool IsVirtualMac(PhysicalAddress ethPacketSourceHardwareAddress)
        {
            var bytes = ethPacketSourceHardwareAddress.GetAddressBytes();
            return bytes[1] == 0 && bytes[3] == 0 && bytes[4] == 0 && bytes[5] == 0;
        }
    }
}