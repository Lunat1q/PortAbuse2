﻿using System.Collections.Generic;
using System.Linq;
using PortAbuse2.Core.Proto;

// ReSharper disable ExplicitCallerInfoArgument

namespace PortAbuse2.Core.Common
{
    public class AppEntry : PaNotified
    {
        private int _hiddenCount;
        private int _tcpConnections;
        private int _udpConnections;
        public string Name { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int InstancePid { get; set; }
        public string Title { get; set; }

        public int HiddenCount
        {
            get => this._hiddenCount;
            set
            {
                this._hiddenCount = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(@"HaveHidden");
            }
        }

        public int TcpConnections
        {
            get => this._tcpConnections;
            set
            {
                if (value == this._tcpConnections) return;
                this._tcpConnections = value;
                this.OnPropertyChanged();
            }
        }

        public int UdpConnections
        {
            get => this._udpConnections;
            set
            {
                if (value == this._udpConnections) return;
                this._udpConnections = value;
                this.OnPropertyChanged();
            }
        }

        public bool HaveHidden => this.HiddenCount > 0;

        public string TitleString => string.IsNullOrEmpty(this.Title) ? "" : $"[{this.Title}]";

        public Port.Port[] AppPort { get; set; }

        public AppEntry()
        {
            this.AppPort = new Port.Port[0];
        }

        public void AddNewPort(uint portNum, Protocol protocol)
        {
            if (portNum == 0) return;
            if (this.AppPort == null) this.AppPort = new Port.Port[0];
            if (this.AppPort.Any(x => x.UPortNumber == portNum)) return;
            
            var t = this.AppPort.ToList();
            t.Add(new Port.Port { UPortNumber = portNum, Protocol = protocol });
            this.AppPort = t.ToArray();
        }

        public override string ToString()
        {
            return $"{this.Name} P: {string.Join(", ", this.AppPort.Select(x => x.ToString()))}";
        }
    }
}
