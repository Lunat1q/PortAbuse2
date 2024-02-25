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
            add => this.AddHandler(BlockButtonClickedEvent, value);
            remove => this.RemoveHandler(BlockButtonClickedEvent, value);
        }

        private static readonly RoutedEvent BlockButtonClickedEvent = EventManager.RegisterRoutedEvent("BlockButtonClicked",
            RoutingStrategy.Bubble, typeof(BlockClickHandler), typeof(BlockInOutMenu));

        public int SecondsToBlock
        {
            get => this._secondsToBlock;
            set
            {
                this._secondsToBlock = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.ItemText));
            }
        }

        public string ItemText => $"{this.SecondsToBlock} sec";

        public BlockInOutMenu()
        {
            this.InitializeComponent();
        }

        private void BlockClicked(object sender, RoutedEventArgs e)
        {
            this.OnBlockButtonClicked(this.SecondsToBlock, BlockMode.BlockAll);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnBlockButtonClicked(int sec, BlockMode direction)
        {
            var newEvent = new BlockEventArgs(BlockButtonClickedEvent, this, sec, direction);
            this.RaiseEvent(newEvent);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.OnBlockButtonClicked(this.SecondsToBlock, BlockMode.BlockInput);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.OnBlockButtonClicked(this.SecondsToBlock, BlockMode.BlockOutput);
        }
    }
}
