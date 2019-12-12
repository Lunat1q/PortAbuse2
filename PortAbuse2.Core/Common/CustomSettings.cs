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
            HiddenIps = new Dictionary<string, List<string>>();
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
            if (!HiddenIps.ContainsKey(appName))
            {
                HiddenIps.Add(appName, new List<string>());
            }
            if (!HiddenIps[appName].Contains(ip))
                HiddenIps[appName].Add(ip);
        }

        public void RemoveHiddenIp(string appName, string ip)
        {
            if (!HiddenIps.ContainsKey(appName)) return;
            if (HiddenIps[appName].Contains(ip))
                HiddenIps[appName].Remove(ip);
        }

        public bool CheckIpHidden(string appName, string ip)
        {
            return HiddenIps.ContainsKey(appName) && HiddenIps[appName].Contains(ip);
        }

        public int CountHiddenIpForApp(string appName)
        {
            if (string.IsNullOrEmpty(appName)) return 0;
            return HiddenIps.ContainsKey(appName) ? HiddenIps[appName].Count : 0;
        }
    }
}
