using System.Globalization;
using System.Windows.Data;

namespace Project.Tools
{
    public class PriceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var russianCulture = new CultureInfo("ru-RU");
            
            if (value is decimal price)
            {
                return price.ToString("N2", russianCulture) + " ₽";
            }

            return "0,00 ₽";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                if (decimal.TryParse(stringValue.Replace(" ₽", ""), NumberStyles.Any, new CultureInfo("ru-RU"), out decimal price))
                {
                    return price;
                }
            }
            
            return 0m;
        }
    }
}