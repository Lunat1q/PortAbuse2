using System.Collections.Generic;
using TiqUtils.Serialize;

namespace PortAbuse2.Core.Common
{
    public class CustomSettings
    {
        public static CustomSettings Instance { get; set; }
        public Dictionary<string, List<string>> HiddenIps { get; }
        public string PreviousInterface { get; set; }

        public CustomSettings()
        {
            this.HiddenIps = new Dictionary<string, List<string>>();
        }

        public static void Load(string data)
        {
            var settings = data.DeserializeDataFromString<CustomSettings>() ?? new CustomSettings();
            Instance = settings;
        }
        public static string SaveToString()
        {
            return Instance.SerializeDataToString();
        }

        public void AddHiddenIp(string appName, string ip)
        {
            if (!this.HiddenIps.ContainsKey(appName))
            {
                this.HiddenIps.Add(appName, new List<string>());
            }
            if (!this.HiddenIps[appName].Contains(ip)) this.HiddenIps[appName].Add(ip);
        }

        public void RemoveHiddenIp(string appName, string ip)
        {
            if (!this.HiddenIps.ContainsKey(appName)) return;
            if (this.HiddenIps[appName].Contains(ip)) this.HiddenIps[appName].Remove(ip);
        }

        public bool CheckIpHidden(string appName, string ip)
        {
            return this.HiddenIps.ContainsKey(appName) && this.HiddenIps[appName].Contains(ip);
        }

        public int CountHiddenIpForApp(string appName)
        {
            if (string.IsNullOrEmpty(appName)) return 0;
            return this.HiddenIps.ContainsKey(appName) ? this.HiddenIps[appName].Count : 0;
        }
    }
}
