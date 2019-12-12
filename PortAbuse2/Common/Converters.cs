using System;
using System.Drawing;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace PortAbuse2.Common
{
    public sealed class CountryImageConverter : IValueConverter
    {
        private static readonly BitmapSource EmptyFlag = (Properties.Resources.ResourceManager.GetObject("empty", Properties.Resources.Culture) as Bitmap).LoadBitmap();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is string imageName))
                return EmptyFlag;
            var bm = Properties.Resources.ResourceManager.GetObject(imageName, Properties.Resources.Culture) as Bitmap;
            
            return bm?.LoadBitmap() ?? EmptyFlag;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    public sealed class ShowBlockTime : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return $"B {value}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
    public sealed class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = (bool)value;
            var param = bool.Parse((string)parameter);

            if (param) val = !val;
            return val ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    public sealed class HugeNumbersConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = (int?)value;
            if (val == null) return null;
            var num = (float) val;

            string resultNum;
            if (num < 1000)
                resultNum = num.ToString(CultureInfo.InvariantCulture);
            else if (num < 100000)
                resultNum = (num/1000).ToString("F2")+"k";
            else if (num < 100000000)
                resultNum = (num/1000000).ToString("F2") + "m";
            else
                resultNum = (num/10E+8).ToString("F2") + "b";

            return resultNum;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    public sealed class ByteSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = (long?)value;
            if (val == null) return null;
            var num = (float)val;

            string resultNum;
            if (num < 512)
                resultNum = num.ToString(CultureInfo.InvariantCulture) + "b";
            else if (num < 1048576)
                resultNum = (num / 1024).ToString("F2") + "Kb";
            else if (num < 536870912)
                resultNum = (num / 1048576).ToString("F2") + "Mb";
            else
                resultNum = (num / 1099511627776).ToString("F2") + "Gb";

            return resultNum;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
