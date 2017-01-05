using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using PortAbuse2.Core.Common;
using PortAbuse2.Core.Port;

namespace PortAbuse2.Applications
{
    internal class AppList
    {
        internal static async Task<ObservableCollection<AppIconEntry>> GetRunningApplications(bool showAll = false)
        {
            var list = new ObservableCollection<AppIconEntry>();

            var myProcess = Process.GetProcesses();
            IEnumerable<Process> procOrder = myProcess.OrderBy(proc => proc.ProcessName);
            var portList = PortMaker.GetNetStatPorts();

            foreach (var item in procOrder)
            {
                var openedPorts = portList.Where(mc => mc.ProcessName == item.ProcessName).ToArray();
                try
                {
                    if (openedPorts.Any() || showAll)
                    {
                        var fullName = "";
                        try
                        {
                            fullName = item.MainModule.FileName;
                        }
                        catch (Win32Exception)
                        {
                            //ignore
                        }

                        list.Add(new AppIconEntry
                        {
                            InstancePid = item.Id,
                            Name = item.ProcessName,
                            Title = item.MainWindowTitle,
                            AppPort = openedPorts,
                            FullName = fullName,
                            HiddenCount = IpHider.CountHidden(item.ProcessName)
                        });
                    }
                }
                catch (Exception)
                {
                    //ignore
                }
                await Task.Delay(0);
            }


            return list;
        }
    }
}
