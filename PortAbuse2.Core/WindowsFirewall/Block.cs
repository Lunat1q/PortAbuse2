using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PortAbuse2.Core.Common;
using PortAbuse2.Core.Result;

namespace PortAbuse2.Core.WindowsFirewall
{
    public static class Block
    {
        public static bool ShutAll = false;
        private static readonly List<ExThread> TdList = new List<ExThread>();
        public static void Do30SecBlock(ResultObject resultObject, bool onlyOut = false, bool onlyIn = false)
        {
            if (resultObject.ShowIp != "")
            {
                DoBlock(resultObject, onlyOut, onlyIn);
                var td = new Thread(() => UnBlockIn30(resultObject)) {Name = "UnBlock30"};
                td.Start();
                TdList.Add(new ExThread(td, DateTime.Now, 30));
            }
        }
        public static void UnBlockIn30(ResultObject resultObject)
        {
            var i = 0;
            while (i < 30 && !ShutAll)
            {
                Thread.Sleep(1000);
                i++;
            }
            DoUnBlock(resultObject);
        }

        public static async Task Wait()
        {
            while (TdList.Any(x => x.td.IsAlive))
            {
                var tooLong = TdList.Where(x => x.tdTime.AddSeconds(30 + 30) < DateTime.Now && x.td.IsAlive);
                foreach (var td in tooLong)
                {
                    td.td.Abort();
                }
                await Task.Delay(500);
            }
        }

        public static void DoBlock(ResultObject resultObject, bool onlyOut = false, bool onlyIn = false)
        {
            if (resultObject == null) return;
            if (resultObject.ShowIp != "")
            {
                var sRemIp = resultObject.ShowIp;
                var blockName = sRemIp + "-BLOCK_PA";
                var args = "advfirewall firewall add rule name=" + blockName + " dir=out interface=any action=block remoteip=" + sRemIp + "/32";
                var args2 = "advfirewall firewall add rule name=" + blockName + " dir=in interface=any action=block remoteip=" + sRemIp + "/32";

                var ps = new ProcessStartInfo
                {
                    Arguments = $"/c start \"notitle\" /B \"netsh.exe\" {args}",
                    FileName = "cmd.exe",
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                //ps.RedirectStandardInput = true;
                //ps.RedirectStandardOutput = true;
                //ps.RedirectStandardError = true;

                var ps2 = new ProcessStartInfo
                {
                    Arguments = $"/c start \"notitle\" /B \"netsh.exe\" {args2}",
                    FileName = "cmd.exe",
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                //ps2.RedirectStandardInput = true;
                //ps2.RedirectStandardOutput = true;
                //ps2.RedirectStandardError = true;

                var p1 = new Process {StartInfo = ps};
                if (!onlyOut)
                    p1.Start();

                var p2 = new Process {StartInfo = ps2};
                if (!onlyIn)
                    p2.Start();

                resultObject.Blocked = true;
            }
        }

        public static void DoUnBlock(ResultObject resultObject)
        {
            if (resultObject == null) return;
            var blockName = resultObject.ShowIp + "-BLOCK_PA";
            var args = "advfirewall firewall delete rule name=" + blockName;

            var ps = new ProcessStartInfo
            {
                Arguments = $"/c start \"notitle\" /B \"netsh.exe\" {args}",
                FileName = "cmd.exe",
                WindowStyle = ProcessWindowStyle.Hidden
            };


            var p1 = new Process {StartInfo = ps};
            p1.Start();

            resultObject.Blocked = false;
        }
    }
}
