using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NetFwTypeLib;
using PortAbuse2.Core.Common;
using PortAbuse2.Core.Result;
using TiqUtils.TypeSpecific;

namespace PortAbuse2.Core.WindowsFirewall;

public static class Block
{
    private const string EndlessBlockSuffix = "-BLOCK_PA_ENDLESS";
    private const string BlockSuffix = "-BLOCK_PA";
    public static bool ShutAll = false;
    private static readonly List<ExThread> TdList = new();
    private static readonly Type RuleType;
    private static readonly Type PolicyType;

    static Block()
    {
#pragma warning disable CA1416
        if (RuleType == null)
        {
            RuleType = Type.GetTypeFromProgID("HNetCfg.FWRule")!;
        }

        if (PolicyType == null)
        {
            PolicyType = Type.GetTypeFromProgID("HNetCfg.FwPolicy2")!;
        }
#pragma warning restore CA1416
        UnBlockAllTemporary();
    }

    public static BlockMode DefaultBlockMode { get; set; }

    public static void DoInSecBlock(ConnectionInformation? connectionInformation,
                                    int sec = 30,
                                    BlockMode blockMode = BlockMode.BlockAll)
    {
        if (connectionInformation?.ShowIp.ToString().Empty() == false)
        {
            DoBlock(connectionInformation, false, blockMode);
            var cts = new CancellationTokenSource(TimeSpan.FromMinutes(1));
            var td = new Thread(() => UnBlockInSeconds(connectionInformation, cts.Token, sec))
            {
                Name = "UnBlock30"
            };
            td.Start();
            TdList.Add(new ExThread(td, cts, DateTime.Now));
        }
    }

    private static void UnBlockInSeconds(ConnectionInformation? connectionInformation, CancellationToken token, int sec)
    {
        var i = 0;
        while (i < sec * 2 && !ShutAll)
        {
            Thread.Sleep(500);
            token.ThrowIfCancellationRequested();
            i++;
        }

        DoUnBlock(connectionInformation, token, false);
    }

    public static async Task Wait()
    {
        while (TdList.Any(x => x.Td.IsAlive))
        {
            var tooLong = TdList.Where(x => x.TdTime.AddSeconds(30 + 30) < DateTime.Now && x.Td.IsAlive);
            foreach (var td in tooLong)
            {
                td.Abort();
            }

            await Task.Delay(500);
        }
    }

    private static void AddRule(string name, string ip, NET_FW_RULE_DIRECTION_ direction)
    {
        var firewallRule = (INetFwRule)Activator.CreateInstance(RuleType)!;
        firewallRule.Action = NET_FW_ACTION_.NET_FW_ACTION_BLOCK;
        firewallRule.Description = $"Used to block all internet access for IP:{ip}";
        firewallRule.RemoteAddresses = ip;
        firewallRule.Direction = direction;
        firewallRule.Enabled = true;
        firewallRule.InterfaceTypes = "All";
        firewallRule.Name = name;
        var firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(PolicyType)!;
        firewallPolicy.Rules.Add(firewallRule);
    }

    public static void DoBlock(ConnectionInformation? connectionInformation, bool endlessBlock, BlockMode blockMode)
    {
        if (connectionInformation == null)
        {
            return;
        }

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

    public static void DoUnBlock(ConnectionInformation? connectionInformation,
                                 CancellationToken token,
                                 bool endlessBlock)
    {
        if (connectionInformation == null)
        {
            return;
        }

        var blockName = connectionInformation.ShowIp + (endlessBlock ? EndlessBlockSuffix : BlockSuffix);
        var firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(PolicyType)!;

        // ReSharper disable once NotDisposedResource
        var i = firewallPolicy.Rules.GetEnumerator();
        while (i.MoveNext())
        {
            token.ThrowIfCancellationRequested();
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

        connectionInformation.Blocked = false;
    }

    private static void UnBlockAllTemporary()
    {
        var firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(PolicyType)!;

        try
        {
            // ReSharper disable once NotDisposedResource
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