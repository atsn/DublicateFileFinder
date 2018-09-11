using System;
using System.Drawing;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace DublicateFileFinder
{
    public class ImgConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                Image image = Image.FromFile((string)value);
                return value;
            }
            catch (Exception)
            {
                Icon icon = Icon.ExtractAssociatedIcon((string)value);
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                    icon.Handle,
                    new Int32Rect(0, 0, icon.Width, icon.Height),
                    BitmapSizeOptions.FromEmptyOptions());
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

    }

    public class PathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
                string valuestring = value.ToString();
                if (valuestring.Length > 50)
                    valuestring = $"...{valuestring.Substring(valuestring.Length - 50)}";
                return valuestring;  
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

    }
}