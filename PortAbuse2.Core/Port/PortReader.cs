using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using PortAbuse2.Core.Common;

namespace PortAbuse2.Core.Port
{
    public static class PortMaker
    {
        public static IEnumerable<AppEntry> GetApplicationsWithPorts()
        {
            var apps = new List<AppEntry>();
            try
            {
                IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
                var test = ipGlobalProperties.GetActiveTcpListeners();

                var tcps = SocketConnectionsReader.GetAllTcpConnections();
                foreach (var tcp in tcps)
                {
                    var app = apps.FirstOrDefault(x => x.InstancePid == tcp.ProcessId);
                    if (app == null)
                    {
                        var newEntry = new AppEntry
                        {
                            InstancePid = tcp.ProcessId,
                            Name = tcp.ProcessName,
                            Title = tcp.Title,
                            AppPort = new[]
                            {
                                new Port{ UPortNumber = tcp.RemotePort, Protocol = "TCPv4" },
                                new Port{ UPortNumber = tcp.LocalPort, Protocol = "TCPv4" }
                            },
                            FullName = tcp.FullName,
                            HiddenCount = IpHider.CountHidden(tcp.ProcessName)
                        };
                        apps.Add(newEntry);
                    }
                    else
                    {
                        var t = app.AppPort.ToList();
                        t.Add(new Port { UPortNumber = tcp.RemotePort, Protocol = "TCPv4" });
                        t.Add(new Port { UPortNumber = tcp.LocalPort, Protocol = "TCPv4" });
                        app.AppPort = t.ToArray();
                    }
                }
                var udps = SocketConnectionsReader.GetAllUdpConnections();
                foreach (var udp in udps)
                {
                    var app = apps.FirstOrDefault(x => x.InstancePid == udp.ProcessId);
                    if (app == null)
                    {
                        var newEntry = new AppEntry
                        {
                            InstancePid = udp.ProcessId,
                            Name = udp.ProcessName,
                            Title = udp.Title,
                            AppPort = new[]
                            {
                                new Port{ UPortNumber = udp.LocalPort, Protocol = "UDPv4" }
                            },
                            FullName = udp.FullName,
                            HiddenCount = IpHider.CountHidden(udp.ProcessName)
                        };
                        apps.Add(newEntry);
                    }
                    else
                    {
                        var t = app.AppPort.ToList();
                        t.Add(new Port { UPortNumber = udp.LocalPort, Protocol = "UDPv4" });
                        app.AppPort = t.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return apps;
        }
    }
}
