using System;
using System.Globalization;
using System.Windows.Data;

namespace Project.Tools
{
    public class PriceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int price)
            {
                var price_str = price.ToString("N0", culture) + " ₽";
                return price_str;
            }

            return "0 ₽";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}