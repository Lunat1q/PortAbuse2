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
            var apps = new List<AppEntry>();
            try
            {
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
                            FullName = tcp.FullName,
                            HiddenCount = IpHider.CountHidden(tcp.ProcessName)
                        };
                        //newEntry.AddNewPort(tcp.RemotePort, tcp.Protocol);
                        newEntry.AddNewPort(tcp.LocalPort, tcp.Protocol);
                        apps.Add(newEntry);
                    }
                    else
                    {
                        //app.AddNewPort(tcp.RemotePort, tcp.Protocol);
                        app.AddNewPort(tcp.LocalPort, tcp.Protocol);
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
                            FullName = udp.FullName,
                            HiddenCount = IpHider.CountHidden(udp.ProcessName)
                        };
                        newEntry.AddNewPort(udp.LocalPort, udp.Protocol);
                        apps.Add(newEntry);
                    }
                    else
                    {
                        app.AddNewPort(udp.LocalPort, udp.Protocol);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return apps.OrderBy(x=>x.Name);
        }
    }
}
