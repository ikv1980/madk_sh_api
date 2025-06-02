using Project.Models;

namespace Project.Tools
{
    // настройка для сортировки в справочниках
    public static class Config
    {
        public static readonly Dictionary<Type, string> DefaultSortProperties = new Dictionary<Type, string>
        {
            { typeof(UserDepartment), nameof(UserDepartment.DepartmentName) },
            { typeof(UserPosition), nameof(UserPosition.PositionName) },
            { typeof(UserStatus), nameof(UserStatus.StatusName) },
            { typeof(CarColor), nameof(CarColor.ColorName) },
        };
    }

    // настройка для поиска в связанных таблицах
    public static class SearchConfig
    {
        public static readonly Dictionary<Type, List<string>> SearchNavigationProperties =
            new Dictionary<Type, List<string>>
            {
                {
                    typeof(Car),
                    new List<string>
                    {
                        "CarColorNavigation.ColorName",
                        "CarCountryNavigation.CountryName",
                        "CarMarkNavigation.MarkName",
                        "CarModelNavigation.ModelName",
                        "CarTypeNavigation.TypeName",
                    }
                },
                {
                    typeof(CarMarkModelCountry),
                    new List<string>
                    {
                        "Mark.MarkName",
                        "Model.ModelName",
                        "Country.CountryName",
                    }
                },
                {
                    typeof(User),
                    new List<string>
                    {
                        "Department.DepartmentName",
                        "Position.PositionName",
                        "Status.StatusName",
                    }
                },
                {
                    typeof(UserDepartmentPosition),
                    new List<string>
                    {
                        "Department.DepartmentName",
                        "Function.PositionName",
                    }
                },
                {
                    typeof(Order),
                    new List<string>
                    {
                        "OrdersClientNavigation.ClientName",
                        "OrdersDeliveryNavigation.DeliveryName",
                        "OrdersPaymentNavigation.PaymentName",
                        "OrdersUserNavigation.Firstname",
                        "OrdersUserNavigation.Surname",
                    }
                },
            };
    }
}