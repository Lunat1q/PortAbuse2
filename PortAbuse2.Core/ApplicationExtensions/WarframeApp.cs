using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using PortAbuse2.Core.Common;
using PortAbuse2.Core.Parser;
using PortAbuse2.Core.Result;

namespace PortAbuse2.Core.ApplicationExtensions
{
    public sealed class WarframeApp : IApplicationExtension
    {
        public IEnumerable<string> AppNames => new[] {"Warframe.x64", "Warframe"};
        private readonly Regex _nameSessionPattern = new Regex("([a-zA-z0-9]{3,64})[\\s]+([a-zA-z0-9]{20,27})");
        private readonly Regex _warframeName = new Regex("(\\/Lotus\\/Powersuits\\/)(\\w+\\/\\w+)");
        private ConcurrentDictionary<string, WarframePlayerData> _sniffedSessions;
        private ConcurrentDictionary<string, ConnectionInformation> _connectionPackageCollection;

        public bool Active { get; set; }
        
        public void Stop()
        {
            this.Active = false;
            this._sniffedSessions = new ConcurrentDictionary<string, WarframePlayerData>();
            this._connectionPackageCollection = new ConcurrentDictionary<string, ConnectionInformation>();
        }

        public void PackageReceived(IPAddress ipDest, IPAddress ipSource, byte[] data, bool direction,
            ConnectionInformation resultobject, PortInformation portInfo)
        {
            if (!this.Active) return;
            if (!resultobject.Resolved)
            {
                this.TryToDetermineSessionHash(resultobject, data);
            }
            var strData = BytesParser.BytesToStringConverted(data, true);
            this.TryHandleWfData(strData, resultobject);
        }

        private static bool ContainsIgnoreCase(string str, string contains)
        {
            return str.IndexOf(contains, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

        private void TryHandleWfData(string strData, ConnectionInformation ro)
        {
            if (this.TryGetWfData(strData) && !ro.Resolved)
            {
                ro.Resolved = true;
                ro.ExtraInfo = "DE Server";
            }
            else if (!ro.Resolved)
            {
                if (strData.Length > 100)
                {
                    var lameSearch = "lotus/powersuits/";
                    var indexOf = strData.IndexOf(lameSearch, StringComparison.CurrentCultureIgnoreCase);
                    if (indexOf >= 0)
                    {
                        var badString = strData.Substring(indexOf + lameSearch.Length, 16);
                        badString = badString.Replace("(", "");
                        var badWarFrameName = badString.Split(' ').FirstOrDefault();
                        if (badWarFrameName != null && !ContainsIgnoreCase(badWarFrameName, "operator") && !ContainsIgnoreCase(badWarFrameName, "npc"))
                            ro.ExtraInfo = badString.Split(' ').FirstOrDefault();
                    }
                }
            }
        }

        private void IdentifyAttempt(string sessionKey)
        {
            if (this._connectionPackageCollection.ContainsKey(sessionKey))
            {
                var ro = this._connectionPackageCollection[sessionKey];
                if (this._sniffedSessions.ContainsKey(sessionKey))
                {
                    var wfData = this._sniffedSessions[sessionKey];
                    ro.ExtraInfo = $"{wfData.Name} - {wfData.Warframe}";
                }
            }
        }

        private bool TryGetWfData(string strData)
        {
            try
            {
                var match = this._warframeName.Match(strData);
                if (match.Success)
                {
                    var warframeName = match.Groups[2].Value.Split('/').Skip(1).FirstOrDefault();
                    match = this._nameSessionPattern.Match(strData);
                    if (match.Success)
                    {
                        var sessionHash = match.Groups[2].Value;
                        var wfData = new WarframePlayerData(match.Groups[1].Value) {Warframe = warframeName};
                        this.AddOrUpdateSession(wfData, sessionHash);
                        this.IdentifyAttempt(sessionHash);
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

        private void TryToDetermineSessionHash(ConnectionInformation resultobject, byte[] data)
        {
            try
            {
                var sessionKey = GetSessionFromInitialPacket(data);
                if (sessionKey == string.Empty) return;
                if (this._connectionPackageCollection.ContainsKey(sessionKey))
                {
                    this._connectionPackageCollection[sessionKey] = resultobject;
                }
                else
                {
                    this._connectionPackageCollection.TryAdd(sessionKey, resultobject);
                }
                resultobject.Resolved = true;
                this.IdentifyAttempt(sessionKey);
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
            if (this._sniffedSessions.ContainsKey(sessionKey))
            {
                this._sniffedSessions[sessionKey] = wfData;
            }
            else
            {
                this._sniffedSessions.TryAdd(sessionKey, wfData);
            }
        }

        public void Start()
        {
            this._sniffedSessions = new ConcurrentDictionary<string, WarframePlayerData>();
            this._connectionPackageCollection = new ConcurrentDictionary<string, ConnectionInformation>();
            this.Active = true;
            //Task.Run(Worker);
        }

        private sealed class WarframePlayerData
        {
            public WarframePlayerData(string name)
            {
                this.Name = name;
            }

            public string Name { get; }
            public string Warframe { get; set; }
            public override string ToString()
            {
                return $"{this.Name} - {this.Warframe}";
            }
        }
    }
}
