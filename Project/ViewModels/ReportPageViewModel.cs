
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Input;
using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Project.Models;
using Project.Tools;

namespace Project.ViewModels
{
    public class ReportPageViewModel : INotifyPropertyChanged
    {
        private List<User> _managers;
        private ulong _selectedManagerId;
        private DateTime? _startDate;
        private DateTime? _endDate;
        private SeriesCollection _salesSeries;
        private List<string> _salesDates;

        public List<User> Managers
        {
            get => _managers;
            set
            {
                _managers = value;
                OnPropertyChanged(nameof(Managers));
            }
        }

        public ulong SelectedManagerId
        {
            get => _selectedManagerId;
            set
            {
                _selectedManagerId = value;
                OnPropertyChanged(nameof(SelectedManagerId));
            }
        }

        public DateTime? StartDate
        {
            get => _startDate;
            set
            {
                _startDate = value;
                OnPropertyChanged(nameof(StartDate));
            }
        }

        public DateTime? EndDate
        {
            get => _endDate;
            set
            {
                _endDate = value;
                OnPropertyChanged(nameof(EndDate));
            }
        }

        public SeriesCollection SalesSeries
        {
            get => _salesSeries;
            set
            {
                _salesSeries = value;
                OnPropertyChanged(nameof(SalesSeries));
            }
        }

        public List<string> SalesDates // Для меток по оси X
        {
            get => _salesDates;
            set
            {
                _salesDates = value;
                OnPropertyChanged(nameof(SalesDates));
            }
        }

        public ICommand GenerateReportCommand { get; }
        public ICommand ExportToExcelCommand { get; }

        public ReportPageViewModel()
        {
            GenerateReportCommand = new RelayCommand(param => GenerateReport());
            ExportToExcelCommand = new RelayCommand(param => ExportToExcel());
            LoadManagers();

            // Первоначальные настройки: [начало года - сегодня]
            StartDate = new DateTime(DateTime.Now.Year, 1, 1);
            EndDate = DateTime.Now;

            // Генерация отчета при инициализации
            GenerateReport();
        }

        // Вывод менеджеров с заказами
        private async void LoadManagers()
        {
            using (var context = new Db())
            {
                var managers = await context.Users
                    .Where(u => u.DepartmentId == 4 &&
                                context.Orders.Any(o => o.UserId == u.Id))
                    .ToListAsync();

                // Добавляем элемент "Все" в начало списка
                Managers = new List<User>
                {
                    new User { Id = 0, Surname = "Все", Firstname = "" }
                }.Concat(managers).ToList();
            }
        }

        private void GenerateReport()
        {
            using (var context = new Db())
            {
                var query = context.Orders
                    .Include(o => o.OrderCars) // Включаем связанные автомобили
                    .ThenInclude(m => m.Car) // Включаем данные об автомобилях
                    .Include(o => o.OrderStatuses) // Включаем статусы заказов
                    .Include(o => o.User) // Включаем данные о менеджере
                    .AsQueryable();

                // Если выбран не "Все", применяем фильтр по менеджеру
                if (SelectedManagerId != 0)
                {
                    query = query.Where(o => o.UserId == SelectedManagerId);
                }

                if (StartDate.HasValue)
                {
                    query = query.Where(o => o.CreatedAt >= StartDate.Value);
                }

                if (EndDate.HasValue)
                {
                    query = query.Where(o => o.CreatedAt <= EndDate.Value);
                }

                // Группируем по дате и менеджеру
                var salesData = query
                    .GroupBy(o => new { o.CreatedAt.Value.Date, o.User.Id })
                    .Select(g => new
                    {
                        Date = g.Key.Date,
                        ManagerId = g.Key.Id,
                        ManagerName = g.First().User.Surname + " " +
                                      g.First().User.Firstname,
                        Total = g.Sum(o => o.OrderCars.Sum(m => (decimal)m.Car.Price))
                    })
                    .ToList();

                // Создаем коллекцию значений для графика
                var seriesCollection = new SeriesCollection();

                // Получаем список всех менеджеров с продажами
                var managers = salesData
                    .Select(d => new { d.ManagerId, d.ManagerName })
                    .Distinct()
                    .ToList();

                // Получаем все уникальные даты из данных
                var allDates = salesData
                    .Select(d => d.Date)
                    .Distinct()
                    .OrderBy(d => d)
                    .ToList();

                // Создаем серию для каждого менеджера
                foreach (var manager in managers)
                {
                    var managerSales = salesData
                        .Where(d => d.ManagerId == manager.ManagerId)
                        .OrderBy(d => d.Date)
                        .ToList();

                    // Создаем список значений для каждой даты
                    var values = new List<decimal>();
                    foreach (var date in allDates)
                    {
                        var sale = managerSales.FirstOrDefault(d => d.Date == date);
                        values.Add(sale != null ? Math.Round(sale.Total) : 0);
                    }

                    // StackedColumnSeries, ColumnSeries, LineSeries
                    seriesCollection.Add(new StackedColumnSeries()
                    {
                        Title = manager.ManagerName, // Имя менеджера в легенде
                        Values = new ChartValues<decimal>(values),
                        DataLabels = true,
                        LabelPoint = point => point.Y.ToString("N0")
                    });
                }

                SalesSeries = seriesCollection;

                // Устанавливаем метки по оси X
                SalesDates = allDates
                    .Select(d => d.ToString("d"))
                    .ToList(); // Форматируем даты
            }
        }

        private void ExportToExcel()
        {
            using (var context = new Db())
            {
                // Получаем базовый запрос с включенными зависимостями
                var query = context.Orders
                    .Include(o => o.Client)
                    .Include(o => o.User)
                    .Include(o => o.OrderCars)
                    .ThenInclude(oc => oc.Car)
                    .Where(o => o.DeletedAt == null)
                    .OrderBy(o => o.CreatedAt)
                    .AsQueryable();

                // Применяем фильтры
                if (SelectedManagerId != 0)
                {
                    query = query.Where(o => o.UserId == SelectedManagerId);
                }

                // Получаем минимальную и максимальную даты для заголовка
                DateTime minDate = StartDate ?? context.Orders
                    .Where(o => o.CreatedAt.HasValue)
                    .Min(o => o.CreatedAt.Value);

                DateTime maxDate = EndDate ?? context.Orders
                    .Where(o => o.CreatedAt.HasValue)
                    .Max(o => o.CreatedAt.Value);

                if (StartDate.HasValue)
                {
                    query = query.Where(o => o.CreatedAt >= StartDate.Value);
                }

                if (EndDate.HasValue)
                {
                    query = query.Where(o => o.CreatedAt <= EndDate.Value);
                }

                var orders = query.ToList();

                // Создаем новый Excel-файл
                using (var package = new ExcelPackage())
                {
                    // Добавляем лист "Отчет по заказам"
                    var worksheet = package.Workbook.Worksheets.Add("Отчет по заказам");

                    // Устанавливаем Times New Roman для всего листа
                    worksheet.Cells.Style.Font.Name = "Times New Roman";
                    worksheet.Cells.Style.Font.Size = 14;
                    
                    // 1. Заполняем шапку отчета
                    // Заголовок "Отчет по продажам ООО "Автосалон""
                    worksheet.Cells["B3"].Value = "Отчет по продажам ООО \"Автосалон\"";
                    worksheet.Cells["B3"].Style.Font.Bold = true;
                    worksheet.Cells["B3"].Style.Font.Size = 16;

                    // Дата составления отчета
                    worksheet.Cells["F3"].Value = DateTime.Now.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
                    worksheet.Cells["F3"].Style.Font.Size = 16;

                    // Устанавливаем светло-серый фон для B3:F3
                    using (var range = worksheet.Cells["B3:F3"])
                    {
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    }

                    // 2. Информация о сотруднике и периоде
                    worksheet.Cells["B5"].Value = "Сотрудник:";
                    worksheet.Cells["B5"].Style.Font.Bold = true;

                    string managerName = SelectedManagerId == 0
                        ? "по всем сотрудникам"
                        : $"{orders.FirstOrDefault()?.User?.Surname} {orders.FirstOrDefault()?.User?.Firstname} {orders.FirstOrDefault()?.User?.Patronymic}"
                            .Trim();

                    worksheet.Cells["C5"].Value = managerName;

                    worksheet.Cells["B6"].Value = "Начало:";
                    worksheet.Cells["B6"].Style.Font.Bold = true;
                    worksheet.Cells["C6"].Value = minDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);

                    worksheet.Cells["B7"].Value = "Конец:";
                    worksheet.Cells["B7"].Style.Font.Bold = true;
                    worksheet.Cells["C7"].Value = maxDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);

                    // 3. Создаем таблицу заказов
                    // Заголовки таблицы
                    worksheet.Cells["B9"].Value = "№ заказа";
                    worksheet.Cells["C9"].Value = "Клиент";
                    worksheet.Cells["D9"].Value = "Менеджер";
                    worksheet.Cells["E9"].Value = "Дата заказа";
                    worksheet.Cells["F9"].Value = "Сумма";

                    // Форматирование заголовков таблицы
                    using (var range = worksheet.Cells["B9:F9"])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(Color.LightCyan);
                        range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    }

                    // Заполняем данные таблицы
                    int startRow = 10;
                    foreach (var order in orders)
                    {
                        worksheet.Cells[$"B{startRow}"].Value = order.Id;
                        worksheet.Cells[$"C{startRow}"].Value = order.Client?.ClientName ?? "N/A";

                        // Формируем ФИО менеджера с проверкой на null
                        string managerFullName = order.User != null
                            ? $"{order.User.Surname} {order.User.Firstname} {order.User.Patronymic}".Trim()
                            : "N/A";
                        worksheet.Cells[$"D{startRow}"].Value = managerFullName;

                        worksheet.Cells[$"E{startRow}"].Value =
                            order.CreatedAt?.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture) ?? "N/A";
    
                        worksheet.Cells[$"F{startRow}"].Value = order.OrderCars.Sum(m => m.Car?.Price ?? 0);
                        worksheet.Cells[$"F{startRow}"].Style.Numberformat.Format = "#,##0.00 ₽";

                        startRow++;
                    }

                    // Применяем стиль ко всему диапазону данных
                    if (startRow > 10) // Если есть данные
                    {
                        var dataRange = worksheet.Cells[$"B10:F{startRow - 1}"];
    
                        // Границы для всех строк с данными
                        dataRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        dataRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        dataRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        dataRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    }

                    // Добавляем строку ИТОГО (26 строк после начала таблицы)
                    int totalRow = startRow + 1;
                    worksheet.Cells[$"E{totalRow}"].Value = "ИТОГО";
                    worksheet.Cells[$"E{totalRow}"].Style.Font.Bold = true;

                    worksheet.Cells[$"F{totalRow}"].Formula = $"SUM(F10:F{startRow - 1})";
                    worksheet.Cells[$"F{totalRow}"].Style.Numberformat.Format = "#,##0.00 ₽";
                    worksheet.Cells[$"F{totalRow}"].Style.Font.Bold = true;

                    // Границы для итоговой строки
                    using (var range = worksheet.Cells[$"F{totalRow}"])
                    {
                        range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    }

                    // Автоподбор ширины столбцов
                    //worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                    // Фиксированная ширина столбца 
                    worksheet.Column(2).Width = 15;
                    worksheet.Column(3).Width = 30;
                    worksheet.Column(4).Width = 40;
                    worksheet.Column(5).Width = 17;
                    worksheet.Column(6).Width = 20;
                    
                    // Диалог сохранения файла
                    var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                    {
                        Filter = "Excel Files (*.xlsx)|*.xlsx",
                        FileName = $"Отчет_по_заказам_{DateTime.Now:yyyyMMdd}.xlsx",
                        DefaultExt = ".xlsx"
                    };

                    if (saveFileDialog.ShowDialog() == true)
                    {
                        // Сохраняем файл
                        var excelFile = new FileInfo(saveFileDialog.FileName);
                        package.SaveAs(excelFile);

                        MessageBox.Show($"Отчет успешно сохранен:\n{excelFile.FullName}",
                            "Успех",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}