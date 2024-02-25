using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PortAbuse2.Controls;
using PortAbuse2.Core.Common;
using PortAbuse2.Core.Result;
using PortAbuse2.Core.WindowsFirewall;
using PortAbuse2.Tools;

namespace PortAbuse2.Styling
{
    public partial class Templates
    {
        private void Block_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (!(btn?.DataContext is ConnectionInformation obj)) return;
            if (!obj.Blocked)
            {
                Block.DoBlock(obj, true, Block.DefaultBlockMode);
                btn.Content = "UnB";
                var unblockColor = btn.FindResource("UnblockColor") as SolidColorBrush;
                btn.Background = unblockColor;
            }
            else
            {
                btn.Content = "B";
                Block.DoUnBlock(obj, CancellationToken.None, true);
                var blockColor = btn.FindResource("BlockColor") as SolidColorBrush;
                btn.Background = blockColor;
            }
        }

        private void Block30Sec_Click(object sender, RoutedEventArgs e)
        {
            BlockFromControl(sender, BlockTimeContainer.CurrentBlockTime, Block.DefaultBlockMode);
        }

        private static void BlockFromControl(object sender, int sec, BlockMode mode)
        {
            if (!TryGetResultObject(sender, out ConnectionInformation? obj)) return;
            Block.DoInSecBlock(obj, sec, mode);
        }

        private void HideThisIpMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var mi = sender as MenuItem;
            var listBox = (mi?.Parent as ContextMenu)?.Tag as ListBox;
            if (listBox?.SelectedIndex == -1) return;
            var selectedItems = GetMultipleSelectedItem<ConnectionInformation>(listBox);
            if (selectedItems == null) return;
            foreach (var ro in selectedItems)
            {
                if (ro.Application != null)
                {
                    if (!ro.Hidden)
                    {
                        CustomSettings.Instance.AddHiddenIp(ro.Application.Name, ro.ShowIp.ToString());
                    }
                    else
                    {
                        CustomSettings.Instance.RemoveHiddenIp(ro.Application.Name, ro.ShowIp.ToString());
                    }
                    ro.Application.HiddenCount = CustomSettings.Instance.CountHiddenIpForApp(ro.Application.Name);
                }
                ro.Hidden = !ro.Hidden;
            }
        }

        private void TraceRtThisIpMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var mi = sender as MenuItem;
            var listBox = (mi?.Parent as ContextMenu)?.Tag as ListBox;
            if (listBox?.SelectedIndex == -1) return;
            var selectedItems = GetMultipleSelectedItem<ConnectionInformation>(listBox);
            if (selectedItems == null) return;
            foreach (var ro in selectedItems)
            {
                var trace = new TraceRoute(ro);
                trace.Show();
            }
        }
        
        private void PingIpMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var mi = sender as MenuItem;
            var listBox = (mi?.Parent as ContextMenu)?.Tag as ListBox;
            if (listBox?.SelectedIndex == -1) return;
            var selectedItems = GetMultipleSelectedItem<ConnectionInformation>(listBox);
            if (selectedItems == null) return;
            foreach (var ro in selectedItems)
            {
                var trace = new PingForm(ro);
                trace.Show();
            }
        }
        
        private void CopyIpMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var mi = sender as MenuItem;
            var listBox = (mi?.Parent as ContextMenu)?.Tag as ListBox;
            if (listBox?.SelectedIndex == -1) return;
            var selectedItems = GetMultipleSelectedItem<ConnectionInformation>(listBox);
            if (selectedItems == null) return;
            Clipboard.SetText(selectedItems.First().ShowIp.ToString());
        }

        private static List<T>? GetMultipleSelectedItem<T>(ListBox? listBox)
        {
            var resultList = new List<T>();
            if (listBox == null) return resultList;
            resultList.AddRange(from object selectedItem in listBox.SelectedItems select (T) selectedItem);
            return resultList;
        }

        private static bool TryGetResultObject(object sender, out ConnectionInformation? obj)
        {
            var control = sender as Control;
            obj = null;
            while (obj == null && control?.Parent != null)
            {
                obj = control.DataContext as ConnectionInformation;
                control = control.Parent as Control;
            }
            return obj != null;
        }

        private void BlockClicked(object sender, BlockEventArgs args)
        {
            BlockFromControl(sender, args.Time, args.Mode);
        }
    }
}
