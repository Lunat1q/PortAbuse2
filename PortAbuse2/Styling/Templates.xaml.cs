using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PortAbuse2.Controls;
using PortAbuse2.Core.Common;
using PortAbuse2.Core.Result;
using PortAbuse2.Core.WindowsFirewall;

namespace PortAbuse2.Styling
{
    public partial class Templates
    {
        private void Block_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var obj = btn?.DataContext as ResultObject;
            if (obj == null) return;
            if (!obj.Blocked)
            {
                Block.DoBlock(obj);
                btn.Content = "UnB";
                var unblockColor = btn.FindResource("UnblockColor") as SolidColorBrush;
                btn.Background = unblockColor;
            }
            else
            {
                btn.Content = "B";
                Block.DoUnBlock(obj);
                var blockColor = btn.FindResource("BlockColor") as SolidColorBrush;
                btn.Background = blockColor;
            }
        }

        private void Block30Sec_Click(object sender, RoutedEventArgs e)
        {
            BlockFromControl(sender, BlockTimeContainer.CurrentBlockTime);
        }

        private static void BlockFromControl(object sender, int sec)
        {
            if (GetResultObject(sender, out ResultObject obj)) return;
            Block.DoInSecBlock(obj, sec);
        }

        private void HideThisIpMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var mi = sender as MenuItem;
            var listBox = (mi?.Parent as ContextMenu)?.Tag as ListBox;
            if (listBox?.SelectedIndex == -1) return;
            var selectedItems = GetMultipleSelectedItem<ResultObject>(listBox);
            if (selectedItems == null) return;
            foreach (var ro in selectedItems)
            {
                if (ro.Application != null)
                {
                    if (!ro.Hidden)
                    {
                        IpHider.Add(ro.Application.Name, ro.ShowIp);
                        ro.Application.HiddenCount = IpHider.CountHidden(ro.Application.Name);
                    }
                    else
                    {
                        IpHider.Remove(ro.Application.Name, ro.ShowIp);
                        ro.Application.HiddenCount = IpHider.CountHidden(ro.Application.Name);
                    }
                }
                ro.Hidden = !ro.Hidden;
            }
        }

        private static List<T> GetMultipleSelectedItem<T>(ListBox listBox)
        {
            var resultList = new List<T>();
            if (listBox == null) return resultList;
            resultList.AddRange(from object selectedItem in listBox.SelectedItems select (T) selectedItem);
            return resultList;
        }

        private void BlockFor5sMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            BlockFromControl(sender, 5);
        }

        private void BlockFor10sMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            BlockFromControl(sender, 10);
        }

        private void BlockFor15sMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            BlockFromControl(sender, 15);
        }

        private void BlockFor30sMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            BlockFromControl(sender, 30);
        }

        private void BlockFor60sMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            BlockFromControl(sender, 60);
        }

        private void BlockFor120sMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            BlockFromControl(sender, 120);
        }

        private void MirrorTraficItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (GetResultObject(sender, out ResultObject obj)) return;
            obj.ReverseEnabled = !obj.ReverseEnabled;
        }

        private static bool GetResultObject(object sender, out ResultObject obj)
        {
            var btn = sender as Control;
            obj = btn?.DataContext as ResultObject;
            if (obj == null) return true;
            return false;
        }
    }
}
