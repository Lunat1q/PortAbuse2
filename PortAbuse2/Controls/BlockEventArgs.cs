using System.Windows;
using PortAbuse2.Core.WindowsFirewall;

namespace PortAbuse2.Controls
{
    public class BlockEventArgs : RoutedEventArgs
    {
        public int Time { get; }
        public BlockMode Mode { get; }

        public BlockEventArgs(int time, BlockMode mode)
        {
            Time = time;
            Mode = mode;
        }

        public BlockEventArgs(RoutedEvent routedEvent, int time, BlockMode mode) : base(routedEvent)
        {
            Time = time;
            Mode = mode;
        }

        public BlockEventArgs(RoutedEvent routedEvent, object source, int time, BlockMode mode) : base(routedEvent, source)
        {
            Time = time;
            Mode = mode;
        }
    }
}