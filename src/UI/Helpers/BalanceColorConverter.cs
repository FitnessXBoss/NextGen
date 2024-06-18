using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace NextGen.src.UI.Helpers
{
    public class BalanceColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal balance)
            {
                return balance < 0 ? Brushes.Red : Brushes.Green;
            }
            return Brushes.Black; // Возвращаем черный, если значение не является числом
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
