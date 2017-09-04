using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PortAbuse2.Core.Common;
using PortAbuse2.Core.Proto;
using PortAbuse2.Core.Result;

namespace PortAbuse2.Core.ApplicationExtensions
{
    public sealed class WarframeApp : IApplicationExtension
    {
        public IEnumerable<string> AppNames => new[] {"Warframe.x64", "Warframe"};
        private readonly Regex _nameSessionPotention = new Regex("([a-zA-z0-9]{3,64})[\\s]+([a-zA-z0-9]{20,27})");
        private readonly Regex _warframeName = new Regex("(\\/Lotus\\/Powersuits\\/)(\\w+\\/\\w+)");
        private Dictionary<string, WarframePlayerData> _sniffedSessions;
        private Dictionary<string, ResultObject> _connectionPackageCollection;

        public bool Active { get; set; }

        public IEnumerable<ResultObject> ResultObjectRef { private get; set; }

        [Obsolete("No longer usefull")]
        private async Task Worker()
        {
            while (Active)
            {
                var visible = ResultObjectRef.Where(x=>!x.Old && !x.Hidden).OrderBy(x=>x.DetectionStamp);
                var i = 2;
                foreach (var hosts in visible)
                {
                    hosts.ExtraInfo = i.ToString();
                    i++;
                }
                await Task.Delay(2000);
            }
        }

        public void Stop()
        {
            Active = false;
            _sniffedSessions = new Dictionary<string, WarframePlayerData>();
            _connectionPackageCollection = new Dictionary<string, ResultObject>();
        }

        public void PackageReceived(IPAddress ipDest, IPAddress ipSource, byte[] data, bool direction,
            ResultObject resultobject, IEnumerable<Tuple<Protocol, ushort>> protocol)
        {
            if (!Active) return;
            if (!resultobject.Resolved)
            {
                TryToDetermineSessionHash(resultobject, data);
            }
            var strData = BytesParser.BytesToStringConverted(data, true);
            TryHandleWfData(strData);
        }

        private void TryHandleWfData(string strData)
        {
            var match = _nameSessionPotention.Match(strData);
            if (match.Success)
            {
                TryAddSession(BuildWfData(strData, match.Groups[1].Value), match.Groups[2].Value);
                IdentifyAttempt(match.Groups[2].Value);
            }
        }

        private void IdentifyAttempt(string sessionKey)
        {
            if (_connectionPackageCollection.ContainsKey(sessionKey))
            {
                var ro = _connectionPackageCollection[sessionKey];
                if (_sniffedSessions.ContainsKey(sessionKey))
                {
                    var wfData = _sniffedSessions[sessionKey];
                    ro.ExtraInfo = $"{wfData.Name} - {wfData.Warframe}";
                }
            }
        }

        private WarframePlayerData BuildWfData(string strData, string name)
        {
            WarframePlayerData wfData = new WarframePlayerData(name);
            try
            {
                var match = _warframeName.Match(strData);
                if (match.Success)
                {
                    wfData.Warframe = match.Groups[2].Value.Split('/').Skip(1).FirstOrDefault();
                }
            }
            catch (Exception)
            {
                //ignore
            }
            return wfData;
        }

        private void TryToDetermineSessionHash(ResultObject resultobject, byte[] data)
        {
            try
            {
                var sessionKey = GetSessionFromInitialPacket(data);
                if (sessionKey == string.Empty) return;
                if (_connectionPackageCollection.ContainsKey(sessionKey))
                {
                    _connectionPackageCollection[sessionKey] = resultobject;
                }
                else
                {
                    _connectionPackageCollection.Add(sessionKey, resultobject);
                }
                resultobject.Resolved = true;
                IdentifyAttempt(sessionKey);
            }
            catch (Exception)
            {
               //ignore
            }
        }

        private static string GetSessionFromInitialPacket(byte[] data)
        {
            if (data.Length != 44) return string.Empty;
            var t = data.Skip(14).Take(12).ToArray();
            return BitConverter.ToString(t).Replace("-", string.Empty).ToLower();
        }

        private void TryAddSession(WarframePlayerData wfData, string sessionKey)
        {
            if (_sniffedSessions.ContainsKey(sessionKey))
            {
                _sniffedSessions[sessionKey] = wfData;
            }
            else
            {
                _sniffedSessions.Add(sessionKey, wfData);
            }
        }

        public void Start()
        {
            _sniffedSessions = new Dictionary<string, WarframePlayerData>();
            _connectionPackageCollection = new Dictionary<string, ResultObject>();
            Active = true;
            //Task.Run(Worker);
        }

        private sealed class WarframePlayerData
        {
            public WarframePlayerData(string name)
            {
                Name = name;
            }

            public string Name { get; }
            public string Warframe { get; set; }

        }
    }
}
