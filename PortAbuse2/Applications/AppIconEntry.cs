using System.Windows.Media;
using PortAbuse2.Common;
using PortAbuse2.Core.Common;

namespace PortAbuse2.Applications
{
    public class AppIconEntry : AppEntry
    {
        private ImageSource _icon;

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
