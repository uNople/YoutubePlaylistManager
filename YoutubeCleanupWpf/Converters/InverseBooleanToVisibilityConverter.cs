using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace YouTubeCleanupWpf.Converters
{
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var convertedValue = false;
            if (value is bool data)
            {
                convertedValue = data;
            }
            return convertedValue ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Visibility.Visible;
        }
    }
}
