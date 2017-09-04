using System.Linq;
using PortAbuse2.Core.Proto;

// ReSharper disable ExplicitCallerInfoArgument

namespace PortAbuse2.Core.Common
{
    public class AppEntry : PaNotified
    {
        private int _hiddenCount;
        public string Name { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int InstancePid { get; set; }
        public string Title { get; set; }

        public int HiddenCount
        {
            get => _hiddenCount;
            set
            {
                _hiddenCount = value;
                OnPropertyChanged();
                OnPropertyChanged(@"HaveHidden");
            }
        }

        public bool HaveHidden => HiddenCount > 0;

        public string TitleString => string.IsNullOrEmpty(Title) ? "" : $"[{Title}]";

        public Port.Port[] AppPort { get; set; }

        public AppEntry()
        {
            AppPort = new Port.Port[0];
        }

        public void AddNewPort(uint portNum, Protocol protocol)
        {
            if (portNum == 0) return;
            if (AppPort == null) AppPort = new Port.Port[0];
            if (AppPort.Any(x => x.UPortNumber == portNum)) return;
            
            var t = AppPort.ToList();
            t.Add(new Port.Port { UPortNumber = portNum, Protocol = protocol });
            AppPort = t.ToArray();
        }
    }
}
