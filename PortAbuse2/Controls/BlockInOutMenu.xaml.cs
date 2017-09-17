using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using PortAbuse2.Annotations;
using PortAbuse2.Core.WindowsFirewall;

namespace PortAbuse2.Controls
{
    /// <summary>
    /// Interaction logic for BlockIOMenu.xaml
    /// </summary>
    public partial class BlockInOutMenu : INotifyPropertyChanged
    {
        private int _secondsToBlock;

        public delegate void BlockClickHandler(object sender, BlockEventArgs args);

        public event BlockClickHandler BlockButtonClicked
        {
            add => AddHandler(BlockButtonClickedEvent, value);
            remove => RemoveHandler(BlockButtonClickedEvent, value);
        }

        private static readonly RoutedEvent BlockButtonClickedEvent = EventManager.RegisterRoutedEvent("BlockButtonClicked",
            RoutingStrategy.Bubble, typeof(BlockClickHandler), typeof(BlockInOutMenu));

        public int SecondsToBlock
        {
            get => _secondsToBlock;
            set
            {
                _secondsToBlock = value; 
                OnPropertyChanged();
                OnPropertyChanged(nameof(ItemText));
            }
        }

        public string ItemText => $"{SecondsToBlock} sec";

        public BlockInOutMenu()
        {
            InitializeComponent();
        }

        private void BlockClicked(object sender, RoutedEventArgs e)
        {
            OnBlockButtonClicked(SecondsToBlock, BlockMode.BlockAll);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnBlockButtonClicked(int sec, BlockMode direction)
        {
            var newEvent = new BlockEventArgs(BlockButtonClickedEvent, this, sec, direction);
            RaiseEvent(newEvent);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OnBlockButtonClicked(SecondsToBlock, BlockMode.BlockInput);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            OnBlockButtonClicked(SecondsToBlock, BlockMode.BlockOutput);
        }
    }
}
