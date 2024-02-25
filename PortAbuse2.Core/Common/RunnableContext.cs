namespace PortAbuse2.Core.Common
{
    public class RunnableContext : PaNotified
    {
        private bool _isRunning;

        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                if (value == _isRunning) return;
                _isRunning = value;
                OnPropertyChanged();
            }
        }
    }
}




