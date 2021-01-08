using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace YouTubeCleanupWpf
{
    public class NullabilityToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is WpfVideoData data)
            {
                if (data.IsDeletedFromYouTube)
                    return Visibility.Hidden;
                return Visibility.Visible;
            }
            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Visibility.Visible;
        }
    }
}
