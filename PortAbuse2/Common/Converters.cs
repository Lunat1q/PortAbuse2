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
            var imageName = value as string;
            if (imageName == null)
                return EmptyFlag;
            var bm = Properties.Resources.ResourceManager.GetObject(imageName, Properties.Resources.Culture) as Bitmap;
            
            return bm?.LoadBitmap() ?? EmptyFlag;
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
}
