using System.Windows.Media;
using PortAbuse2.Common;
using PortAbuse2.Core.Port;

namespace PortAbuse2.Applications
{
    public class AppEntry
    {
        public string Name { get; set; }
        public string FullName { get; set; }
        public int InstancePid { get; set; }
        public string Title { get; set; }

        public string TitleString => string.IsNullOrEmpty(Title) ? "" : $"[{Title}]";

        public Port[] AppPort { get; set; }

        private ImageSource _icon;

        public ImageSource Icon
        {
            get
            {
                return _icon ?? Images.LoadImageSourceFromResource("empty.png");
            }
            set { _icon = value; }
        }

        public AppEntry()
        {
            AppPort = new Port[0];
        }
    }
}
