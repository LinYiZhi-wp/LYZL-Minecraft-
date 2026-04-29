using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using GeminiLauncher.Services;

namespace GeminiLauncher.Converters
{
    public class CachedImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string url && !string.IsNullOrEmpty(url))
            {
                var cached = ImageCache.GetOrLoadAsync(url, 180).Result;
                return cached ?? new BitmapImage();
            }
            return new BitmapImage();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
