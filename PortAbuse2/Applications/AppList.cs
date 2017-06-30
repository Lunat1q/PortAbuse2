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
    internal static class AppList
    {
        internal static async Task<ObservableCollection<AppIconEntry>> GetRunningApplications(bool showAll = false)
        {
            var list = new ObservableCollection<AppIconEntry>();

            var portList = PortMaker.GetApplicationsWithPorts();

            foreach (var item in portList)
            {
                try
                {
                    if (item.AppPort.Any() || showAll)
                    {
                        list.Add(new AppIconEntry
                        {
                            InstancePid = item.InstancePid,
                            Name = item.Name,
                            Title = item.Title,
                            AppPort = item.AppPort,
                            FullName = item.FullName,
                            HiddenCount = IpHider.CountHidden(item.Name)
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
