using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Project.Models;
using Project.Tools;

namespace Project.Tools
{
    public class TotalPriceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ulong orderId)
            {
                using (var db = new Db())
                {
                    var totalPrice = db.OrderCars
                        .Where(oc => oc.OrderId == orderId)
                        .Sum(oc => oc.Car.Price);

                    return totalPrice.ToString("N0", culture) + " ₽";
                }
            }

            return "0 ₽";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}