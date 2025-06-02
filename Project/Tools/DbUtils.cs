using System.Windows;
using Microsoft.EntityFrameworkCore;
using Project.Models;
using MessageBox = System.Windows.MessageBox;
using LinqExpression = System.Linq.Expressions.Expression;

namespace Project.Tools
{
    internal static class DbUtils
    {
        public static Db db;

        static DbUtils()
        {
            try
            {
                db = new Db();
            }
            catch (Exception e)
            {
                MessageBox.Show($"Ошибка подключения к БД\n{e}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Поиск в таблице
        public static async Task<List<T>> GetSearchingValues<T>(string searchText) where T : class
        {
            var parameter = LinqExpression.Parameter(typeof(T), "e");

            // Получаем строковые свойства текущей таблицы
            var stringProperties = typeof(T).GetProperties()
                .Where(p => p.PropertyType == typeof(string) && !p.GetGetMethod().IsStatic);

            LinqExpression orExpression = null;

            // Добавляем условия для строковых полей текущей таблицы
            foreach (var prop in stringProperties)
            {
                var propertyExpression = LinqExpression.Property(parameter, prop);
                var likeExpression = LinqExpression.Call(
                    propertyExpression,
                    nameof(string.Contains),
                    Type.EmptyTypes,
                    LinqExpression.Constant(searchText, typeof(string))
                );

                orExpression = orExpression == null
                    ? likeExpression
                    : LinqExpression.OrElse(orExpression, likeExpression);
            }

            // Подключаем навигационные свойства из конфигурации
            if (SearchConfig.SearchNavigationProperties.TryGetValue(typeof(T), out var navigationPaths))
            {
                foreach (var path in navigationPaths)
                {
                    // Разбиваем путь на части
                    var properties = path.Split('.');
                    LinqExpression currentExpression = parameter;

                    // Построение вложенного выражения
                    foreach (var propName in properties)
                    {
                        currentExpression = LinqExpression.Property(currentExpression, propName);
                    }

                    // Добавляем условие Contains для конечного свойства
                    if (currentExpression.Type == typeof(string))
                    {
                        var likeExpression = LinqExpression.Call(
                            currentExpression,
                            nameof(string.Contains),
                            Type.EmptyTypes,
                            LinqExpression.Constant(searchText, typeof(string))
                        );

                        orExpression = orExpression == null
                            ? likeExpression
                            : LinqExpression.OrElse(orExpression, likeExpression);
                    }
                }
            }

            if (orExpression == null)
                return new List<T>();

            var lambda = LinqExpression.Lambda<Func<T, bool>>(orExpression, parameter);

            // Строим запрос с учетом `Include` для связанных сущностей
            var query = db.Set<T>().AsQueryable();
            if (SearchConfig.SearchNavigationProperties.TryGetValue(typeof(T), out var includePaths))
            {
                foreach (var path in includePaths.Select(p => p.Split('.').First()))
                {
                    query = query.Include(path);
                }
            }

            return await query.Where(lambda).ToListAsync();
        }

        // Подсчет всех записей в таблице
        public static async Task<int> GetTableCount<TTable>() where TTable : class
        {
            using (var context = new Db())
            {
                return await context.Set<TTable>().CountAsync();
            }
        }

        // Получение данных из БД
        public static async Task<List<TTable>> GetTablePagedValuesWithIncludes<TTable>(int page, int pageSize,
            string sortPropertyName = null) where TTable : class
        {
            using (var context = new Db())
            {
                var query = context.Set<TTable>().AsQueryable();

                // Список таблиц, для которых не нужно загружать связанные сущности
                var excludedTypes = new[]
                {
                    typeof(Client),
                    typeof(Delivery),
                    typeof(Payment),
                    typeof(OrderStatus),
                    typeof(CarCountry),
                    typeof(CarMark),
                    typeof(CarModel),
                    typeof(CarType),
                    typeof(CarColor),
                    typeof(UserDepartment),
                    typeof(UserPosition),
                    typeof(Status),
                };

                // Исключаем связанные сущности
                if (!excludedTypes.Contains(typeof(TTable)))
                {
                    var entityType = context.Model.FindEntityType(typeof(TTable));
                    var navigationProperties = entityType.GetNavigations()
                        .Select(n => n.Name)
                        .ToList();

                    // Применяем Include для загрузки всех связанных сущностей
                    foreach (var property in navigationProperties)
                    {
                        query = query.Include(property);
                    }
                }

                // Применяем AsSplitQuery для разделения запросов
                query = query.AsSplitQuery();

                // Применяем сортировку, если указано поле
                if (!string.IsNullOrEmpty(sortPropertyName))
                {
                    var sortProperty = typeof(TTable).GetProperty(sortPropertyName);
                    if (sortProperty != null)
                    {
                        query = query.OrderBy(e => EF.Property<object>(e, sortPropertyName));
                    }
                }

                return await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
        }
    }
}