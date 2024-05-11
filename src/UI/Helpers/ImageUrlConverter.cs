using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace NextGen.src.UI.Helpers
{
    public class ImageUrlConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var url = value as string;
            if (url == null) return null;

            return new BitmapImage(new Uri(url, UriKind.Absolute));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
