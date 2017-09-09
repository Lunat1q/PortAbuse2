using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PortAbuse2.Core.Common;
using PortAbuse2.Core.Result;
using NetFwTypeLib;

namespace PortAbuse2.Core.WindowsFirewall
{
    public static class Block
    {
        public static bool ShutAll = false;
        private static readonly List<ExThread> TdList = new List<ExThread>();
        public static void DoInSecBlock(ResultObject resultObject, int sec = 30, bool onlyOut = false, bool onlyIn = false)
        {
            if (resultObject.ShowIp != "")
            {
                DoBlock(resultObject, onlyOut, onlyIn);
                var td = new Thread(() => UnBlockInSeconds(resultObject, sec)) {Name = "UnBlock30"};
                td.Start();
                TdList.Add(new ExThread(td, DateTime.Now, sec));
            }
        }

        private static void UnBlockInSeconds(ResultObject resultObject, int sec)
        {
            var i = 0;
            while (i < sec * 2 && !ShutAll)
            {
                Thread.Sleep(500);
                i++;
            }
            DoUnBlock(resultObject);
        }

        public static async Task Wait()
        {
            while (TdList.Any(x => x.Td.IsAlive))
            {
                var tooLong = TdList.Where(x => x.TdTime.AddSeconds(30 + 30) < DateTime.Now && x.Td.IsAlive);
                foreach (var td in tooLong)
                {
                    td.Td.Abort();
                }
                await Task.Delay(500);
            }
        }

        private static void AddRule(string name, string ip, NET_FW_RULE_DIRECTION_ direction)
        {
            var firewallRule = (INetFwRule)Activator.CreateInstance(
                Type.GetTypeFromProgID("HNetCfg.FWRule"));
            firewallRule.Action = NET_FW_ACTION_.NET_FW_ACTION_BLOCK;
            firewallRule.Description = $"Used to block all internet access for IP:{ip}";
            firewallRule.RemoteAddresses = ip;
            firewallRule.Direction = direction;
            firewallRule.Enabled = true;
            firewallRule.InterfaceTypes = "All";
            firewallRule.Name = name;
            var firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(
                Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
            firewallPolicy.Rules.Add(firewallRule);
        }

        public static void DoBlock(ResultObject resultObject, bool onlyOut = false, bool onlyIn = false)
        {
            if (resultObject == null) return;
            if (resultObject.ShowIp != "")
            {
                var sRemIp = resultObject.ShowIp;
                var blockName = sRemIp + "-BLOCK_PA";

                if (!onlyOut)
                {
                    AddRule(blockName, sRemIp, NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN);
                }
                if (!onlyIn)
                {
                    AddRule(blockName, sRemIp, NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT);
                }

                resultObject.Blocked = true;
            }
        }

        public static void DoUnBlock(ResultObject resultObject)
        {
            if (resultObject == null) return;
            var blockName = resultObject.ShowIp + "-BLOCK_PA";
            var firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(
                Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));

            try
            {
                var i = firewallPolicy.Rules.GetEnumerator();
                while (i.MoveNext())
                {
                    try
                    {
                        var cur = i.Current as INetFwRule;
                        if (cur != null && cur.Name == blockName)
                        {
                            firewallPolicy.Rules.Remove(cur.Name);
                        }
                    }
                    catch (FileNotFoundException)
                    {
                       //ignore
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            resultObject.Blocked = false;
        }
    }
}
