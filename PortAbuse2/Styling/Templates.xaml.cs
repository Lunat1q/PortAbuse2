using System.Collections.ObjectModel;
using System.Runtime.Remoting;
using System.Windows.Controls;
using System.Windows.Media;
using PortAbuse2.Core.Result;
using PortAbuse2.Core.WindowsFirewall;

namespace PortAbuse2.Styling
{
    public partial class Templates
    {
        private void Block_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var btn = sender as Button;
            var obj = btn?.DataContext as ResultObject;
            if (obj == null) return;
            if (!obj.Blocked)
            {
                Block.DoBlock(obj);
                btn.Content = "UnBlock";
                var unblockColor = btn.FindResource("UnblockColor") as SolidColorBrush;
                btn.Background = unblockColor;
            }
            else
            {
                btn.Content = "Block";
                Block.DoUnBlock(obj);
                var blockColor = btn.FindResource("BlockColor") as SolidColorBrush;
                btn.Background = blockColor;
            }
        }

        private void Block30Sec_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var btn = sender as Button;
            var obj = btn?.DataContext as ResultObject;
            if (obj == null) return;
            Block.DoInSecBlock(obj);
        }
    }
}
