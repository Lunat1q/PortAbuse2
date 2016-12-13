﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using PortAbuse2.Core.Port;

namespace PortAbuse2.Applications
{
    internal class AppList
    {
        internal static async Task<ObservableCollection<AppEntry>> GetRunningApplications(bool showAll = false)
        {
            var list = new ObservableCollection<AppEntry>();

            var myProcess = Process.GetProcesses();
            IEnumerable<Process> procOrder = myProcess.OrderBy(proc => proc.ProcessName);
            var portList = PortMaker.GetNetStatPorts();

            foreach (var item in procOrder)
            {
                var openedPorts = portList.Where(mc => mc.ProcessName == item.ProcessName).Select(x=>x.PortNumber).ToList();
                try
                {
                    if (openedPorts.Any() || showAll)
                        list.Add(new AppEntry
                        {
                            InstancePid = item.Id,
                            Name = item.ProcessName,
                            Title = item.MainWindowTitle,
                            FullName = item.MainModule.FileName,
                            Ports = openedPorts
                        });
                }
                catch (Exception ex)
                {
                    //ignore
                }
                await Task.Delay(0);
            }


            return list;
        }
    }
}
