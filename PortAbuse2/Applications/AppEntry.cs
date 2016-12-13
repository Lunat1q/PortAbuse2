using System.Collections.Generic;
using System.Windows.Media;
using PortAbuse2.Common;

namespace PortAbuse2.Applications
{
    public class AppEntry
    {
        public string Name { get; set; }
        public string FullName { get; set; }
        public int InstancePid { get; set; }
        public string Title { get; set; }
        public List<string> Ports { get; set; }

        public string TitleString => string.IsNullOrEmpty(Title) ? "" : $"[{Title}]";

        private ImageSource _icon = null;

        public ImageSource Icon
        {
            get
            {
                return _icon ?? Images.LoadImageSourceFromResource("empty.png");
            }
            set { _icon = value; }
        }
    }
}
