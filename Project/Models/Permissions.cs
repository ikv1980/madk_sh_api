namespace Project.Tools
{
    // Права по умолчанию
    public class DefaultPermissions
    {
        public static string User { get; } = @"{
            ""Tabs"": [
                { ""Name"": ""user"", ""RusName"": ""Пользователь"", ""Permissions"": { ""Read"": true, ""Write"": true } },
                { ""Name"": ""order"", ""RusName"": ""Заказы"", ""Permissions"": { ""Read"": false, ""Write"": false } },
                { ""Name"": ""report"", ""RusName"": ""Отчеты"", ""Permissions"": { ""Read"": false, ""Write"": false } },
                { ""Name"": ""setting"", ""RusName"": ""Настройки"", ""Permissions"": { ""Read"": false, ""Write"": false } },
                { ""Name"": ""dict"", ""RusName"": ""Словари"", ""Permissions"": { ""Read"": false, ""Write"": false } }
            ],
            ""Directories"": [
                { ""Name"": ""Order"", ""RusName"": ""Заказы"", ""Permissions"": { ""Read"": false, ""Write"": false } },
                { ""Name"": ""Client"", ""RusName"": ""Клиенты"", ""Permissions"": { ""Read"": false, ""Write"": false } },
                { ""Name"": ""Delivery"", ""RusName"": ""Типы доставки"", ""Permissions"": { ""Read"": false, ""Write"": false } },
                { ""Name"": ""Payment"", ""RusName"": ""Типы оплаты"", ""Permissions"": { ""Read"": false, ""Write"": false } },
                { ""Name"": ""OrderStatus"", ""RusName"": ""Статусы заказа"", ""Permissions"": { ""Read"": false, ""Write"": false } },
                
                { ""Name"": ""Car"", ""RusName"": ""Автомобили"", ""Permissions"": { ""Read"": false, ""Write"": false } },
                { ""Name"": ""CarMark"", ""RusName"": ""Марки"", ""Permissions"": { ""Read"": false, ""Write"": false } },
                { ""Name"": ""CarModel"", ""RusName"": ""Модели"", ""Permissions"": { ""Read"": false, ""Write"": false } },
                { ""Name"": ""CarCountry"", ""RusName"": ""Страны"", ""Permissions"": { ""Read"": false, ""Write"": false } },
                { ""Name"": ""CarType"", ""RusName"": ""Типы кузова"", ""Permissions"": { ""Read"": false, ""Write"": false } },
                { ""Name"": ""CarColor"", ""RusName"": ""Цвета"", ""Permissions"": { ""Read"": false, ""Write"": false } },
                { ""Name"": ""CarMarkModelCountry"", ""RusName"": ""Марка - Модель"", ""Permissions"": { ""Read"": false, ""Write"": false } },

                { ""Name"": ""User"", ""RusName"": ""Сотрудники"", ""Permissions"": { ""Read"": false, ""Write"": false } },
                { ""Name"": ""UserDepartment"", ""RusName"": ""Отделы"", ""Permissions"": { ""Read"": false, ""Write"": false } },
                { ""Name"": ""UserPosition"", ""RusName"": ""Должности"", ""Permissions"": { ""Read"": false, ""Write"": false } },
                { ""Name"": ""UserStatus"", ""RusName"": ""Статусы"", ""Permissions"": { ""Read"": false, ""Write"": false } },
                { ""Name"": ""UserDepartmentPosition"", ""RusName"": ""Отдел - Должность"", ""Permissions"": { ""Read"": false, ""Write"": false } }
            ]
        }";
    }

    public class UserPermissions
    {
        public List<TabPermission> Tabs { get; set; }
        public List<DirectoryPermission> Directories { get; set; }
    }

    // Вкладки
    public class TabPermission
    {
        public string Name { get; set; }
        public string RusName { get; set; }
        public Permission Permissions { get; set; }
    }

    // Справочники
    public class DirectoryPermission
    {
        public string Name { get; set; }
        public string RusName { get; set; }
        public Permission Permissions { get; set; }
    }

    public class Permission
    {
        public bool Read { get; set; }
        public bool Write { get; set; }
    }
}