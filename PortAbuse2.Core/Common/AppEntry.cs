namespace PortAbuse2.Core.Common
{
    public class AppEntry : PaNotified
    {
        private int _hiddenCount;
        public string Name { get; set; }
        public string FullName { get; set; }
        public int InstancePid { get; set; }
        public string Title { get; set; }

        public int HiddenCount
        {
            get { return _hiddenCount; }
            set
            {
                _hiddenCount = value;
                OnPropertyChanged();
                OnPropertyChanged("HaveHidden");
            }
        }

        public bool HaveHidden => HiddenCount > 0;

        public string TitleString => string.IsNullOrEmpty(Title) ? "" : $"[{Title}]";

        public Port.Port[] AppPort { get; set; }

        public AppEntry()
        {
            AppPort = new Port.Port[0];
        }
    }
}
