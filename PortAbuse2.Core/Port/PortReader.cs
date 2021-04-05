using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PortAbuse2.Core.Common;

namespace PortAbuse2.Core.Port
{
    public static class PortMaker
    {
        public static IEnumerable<AppEntry> GetApplicationsWithPorts()
        {
            var apps = new Dictionary<int, AppEntry>();
            try
            {
                var tcps = SocketConnectionsReader.GetAllTcpConnections();
                foreach (var tcp in tcps)
                {
                    if (tcp.ProcessName == null && tcp.FullName == null) continue;
                    if (!apps.TryGetValue(tcp.ProcessId, out var app))
                    {
                        app = new AppEntry
                        {
                            InstancePid = tcp.ProcessId,
                            Name = tcp.ProcessName,
                            Title = tcp.Title,
                            FullName = tcp.FullName,
                            HiddenCount = CustomSettings.Instance.CountHiddenIpForApp(tcp.ProcessName)
                        };
                        app.AddNewPort(tcp.LocalPort, tcp.Protocol);
                        apps.Add(tcp.ProcessId, app);
                    }
                    else
                    {
                        app.AddNewPort(tcp.LocalPort, tcp.Protocol);
                    }

                    app.TcpConnections++;
                }

                var udps = SocketConnectionsReader.GetAllUdpConnections();
                foreach (var udp in udps)
                {
                    if (!apps.TryGetValue(udp.ProcessId, out var app))
                    {
                        app = new AppEntry
                        {
                            InstancePid = udp.ProcessId,
                            Name = udp.ProcessName,
                            Title = udp.Title,
                            FullName = udp.FullName,
                            HiddenCount = CustomSettings.Instance.CountHiddenIpForApp(udp.ProcessName)
                        };
                        app.AddNewPort(udp.LocalPort, udp.Protocol);
                        apps.Add(udp.ProcessId, app);
                    }
                    else
                    {
                        app.AddNewPort(udp.LocalPort, udp.Protocol);
                    }

                    app.UdpConnections++;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return apps.Values.OrderBy(x => x.Name);
        }
    }
}
