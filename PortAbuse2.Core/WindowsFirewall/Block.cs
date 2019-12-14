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
        public static BlockMode DefaultBlockMode { get; set; }
        private const string EndlessBlockSuffix = "-BLOCK_PA_ENDLESS";
        private const string BlockSuffix = "-BLOCK_PA";
        public static bool ShutAll = false;
        private static readonly List<ExThread> TdList = new List<ExThread>();
        private static readonly Type RuleType;
        private static readonly Type PolicyType;

        static Block()
        {
            if (RuleType == null)
            {
                RuleType = Type.GetTypeFromProgID("HNetCfg.FWRule");
            }
            if (PolicyType == null)
            {
                PolicyType = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
            }
            UnBlockAllTemporary();
        }

        public static void DoInSecBlock(ConnectionInformation connectionInformation, int sec = 30, BlockMode blockMode = BlockMode.BlockAll)
        {
            if (connectionInformation.ShowIp.ToString() != "")
            {
                DoBlock(connectionInformation, false, blockMode);
                var td = new Thread(() => UnBlockInSeconds(connectionInformation, sec))
                {
                    Name = "UnBlock30"
                };
                td.Start();
                TdList.Add(new ExThread(td, DateTime.Now));
            }
        }

        private static void UnBlockInSeconds(ConnectionInformation connectionInformation, int sec)
        {
            var i = 0;
            while (i < sec * 2 && !ShutAll)
            {
                Thread.Sleep(500);
                i++;
            }
            DoUnBlock(connectionInformation, false);
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
            var firewallRule = (INetFwRule)Activator.CreateInstance(RuleType);
            firewallRule.Action = NET_FW_ACTION_.NET_FW_ACTION_BLOCK;
            firewallRule.Description = $"Used to block all internet access for IP:{ip}";
            firewallRule.RemoteAddresses = ip;
            firewallRule.Direction = direction;
            firewallRule.Enabled = true;
            firewallRule.InterfaceTypes = "All";
            firewallRule.Name = name;
            var firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(PolicyType);
            firewallPolicy.Rules.Add(firewallRule);
        }

        public static void DoBlock(ConnectionInformation connectionInformation, bool endlessBlock, BlockMode blockMode)
        {
            if (connectionInformation == null) return;
            var sRemIp = connectionInformation.ShowIp.ToString();
            if (sRemIp != "")
            {
                var blockName = sRemIp + (endlessBlock ? EndlessBlockSuffix : BlockSuffix);

                if (blockMode == BlockMode.BlockAll || blockMode == BlockMode.BlockInput)
                {
                    AddRule(blockName, sRemIp, NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN);
                }
                if (blockMode == BlockMode.BlockAll || blockMode == BlockMode.BlockOutput)
                {
                    AddRule(blockName, sRemIp, NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT);
                }
                connectionInformation.Blocked = true;
            }
        }

        public static void DoUnBlock(ConnectionInformation connectionInformation, bool endlessBlock)
        {
            if (connectionInformation == null) return;
            var blockName = connectionInformation.ShowIp + (endlessBlock ? EndlessBlockSuffix : BlockSuffix);
            var firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(PolicyType);

            try
            {
                var i = firewallPolicy.Rules.GetEnumerator();
                while (i.MoveNext())
                {
                    try
                    {
                        if (i.Current is INetFwRule cur && cur.Name == blockName)
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
            connectionInformation.Blocked = false;
        }

        private static void UnBlockAllTemporary()
        {
            var firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(PolicyType);

            try
            {
                var i = firewallPolicy.Rules.GetEnumerator();
                while (i.MoveNext())
                {
                    try
                    {
                        if (i.Current is INetFwRule cur && cur.Name.EndsWith(BlockSuffix))
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
        }
    }
}
