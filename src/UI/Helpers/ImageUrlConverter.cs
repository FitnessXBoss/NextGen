using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Diagnostics;

namespace NextGen.src.UI.Helpers
{
    public class ImageUrlConverter : IValueConverter
    {
        private static readonly string DefaultImageUrl = "https://i.ibb.co/hsPFKrr/free-icon-page-not-found-4380687.png";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Debug.WriteLine($"ImageUrlConverter: Value = {value}");

            if (value == null)
            {
                Debug.WriteLine("ImageUrlConverter: Value is null, returning default image");
                return new BitmapImage(new Uri(DefaultImageUrl));
            }

            try
            {
                var imageUrl = value.ToString();
                Debug.WriteLine($"ImageUrlConverter: Converting {imageUrl}");
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    return new BitmapImage(new Uri(imageUrl, UriKind.RelativeOrAbsolute));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ImageUrlConverter: Exception - {ex.Message}");
            }

            Debug.WriteLine("ImageUrlConverter: Returning default image due to error");
            return new BitmapImage(new Uri(DefaultImageUrl));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
