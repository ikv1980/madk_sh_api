using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Project.Models;

// Находим последний статус заказа
namespace Project.Tools
{
    public class OrderLastStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ulong orderId)
            {
                using (var db = new Db())
                {
                    var lastStatus = db.OrderStatuses // Здесь изменено: OrderStatuses → OrderStatus
                        .Where(s => s.OrderId == orderId)
                        .OrderByDescending(s => s.CreatedAt) // Предположил, что дата хранится в CreatedAt
                        .Select(s => s.Status)
                        .FirstOrDefault();

                    return lastStatus?.StatusName ?? "Нет статуса";
                }
            }

            return "Нет статуса"; // Возвращаем значение по умолчанию
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}