using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
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
            TryHandleWfData(strData, resultobject);
        }

        private void TryHandleWfData(string strData, ResultObject ro)
        {
            if (TryGetWfData(strData) && !ro.Resolved)
            {
                ro.Resolved = true;
                ro.ExtraInfo = "DE Server";
            }
            else if (!ro.Resolved)
            {
                if (strData.Length > 100)
                {
                    var lameSearch = "/powersuits/";
                    var indexOf = strData.IndexOf(lameSearch, StringComparison.CurrentCultureIgnoreCase);
                    if (indexOf > 0)
                    {
                        var badString = strData.Substring(indexOf + lameSearch.Length, 16);
                        badString = badString.Replace("(", "");
                        ro.ExtraInfo = badString.Split(' ').FirstOrDefault();
                    }
                }
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

        private bool TryGetWfData(string strData)
        {
            try
            {
                var match = _warframeName.Match(strData);
                if (match.Success)
                {
                    var warframeName = match.Groups[2].Value.Split('/').Skip(1).FirstOrDefault();
                    match = _nameSessionPotention.Match(strData);
                    if (match.Success)
                    {
                        var sessionHash = match.Groups[2].Value;
                        var wfData = new WarframePlayerData(match.Groups[1].Value) {Warframe = warframeName};
                        AddOrUpdateSession(wfData, sessionHash);
                        IdentifyAttempt(sessionHash);
                        return true;

                    }
                }
            }
            catch (Exception)
            {
                //ignore
            }
            return false;
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
            if (data.Skip(44 - 5).Any(x => x != 170)) return string.Empty; //TODO:Check that logic
            var t = data.Skip(14).Take(12).ToArray();
            return BitConverter.ToString(t).Replace("-", string.Empty).ToLower();
        }

        private void AddOrUpdateSession(WarframePlayerData wfData, string sessionKey)
        {
            if (wfData == null) return;
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
            public override string ToString()
            {
                return $"{Name} - {Warframe}";
            }
        }
    }
}
