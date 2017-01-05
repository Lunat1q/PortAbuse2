using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PortAbuse2.Core.Common.Serializer;

namespace PortAbuse2.Core.Common
{
    public static class IpHider
    {
        private const string FileName = "iptohide.json";
        private static readonly string CurFolder = Environment.CurrentDirectory;
        public static Dictionary<string, List<string>> HiddenIps = new Dictionary<string, List<string>>();

        public static void Load()
        {
            var loaded = Json.DeserializeData<Dictionary<string, List<string>>>(Path.Combine(CurFolder, FileName));
            if (loaded != null)
                HiddenIps = loaded;
        }
        public static void Save()
        {
            HiddenIps.SerializeData(Path.Combine(CurFolder, FileName));
        }

        public static void Add(string appName, string ip)
        {
            if (!HiddenIps.ContainsKey(appName))
            {
                HiddenIps.Add(appName, new List<string>());
            }
            if (!HiddenIps[appName].Contains(ip))
                HiddenIps[appName].Add(ip);
        }
        public static bool Check(string appName, string ip)
        {
            return HiddenIps.ContainsKey(appName) && HiddenIps[appName].Contains(ip);
        }

        public static int CountHidden(string appName)
        {
            return HiddenIps.ContainsKey(appName) ? HiddenIps[appName].Count : 0;
        }
    }
}
