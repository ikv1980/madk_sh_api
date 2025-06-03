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
                        "Color.ColorName",
                        "Country.CountryName",
                        "Mark.MarkName",
                        "Model.ModelName",
                        "Type.TypeName",
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
                        "Position.PositionName",
                    }
                },
                {
                    typeof(Order),
                    new List<string>
                    {
                        "Client.ClientName",
                        "Delivery.DeliveryName",
                        "Payment.PaymentName",
                        "User.Firstname",
                        "User.Surname",
                    }
                },
            };
    }
}