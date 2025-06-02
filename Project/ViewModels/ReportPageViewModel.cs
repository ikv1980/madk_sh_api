using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
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
                var query = context.Orders
                    .Include(o => o.Client) // Включаем данные клиента
                    .Include(o => o.User) // Включаем данные менеджера
                    .Include(o => o.OrderCars) // Включаем связанные автомобили
                    .ThenInclude(m => m.Car) // Включаем данные об автомобилях
                    .AsQueryable();

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

                var orders = query.ToList();

                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Отчет по заказам");

                    worksheet.Cells[1, 1].Value = "ID заказа";
                    worksheet.Cells[1, 2].Value = "Клиент";
                    worksheet.Cells[1, 3].Value = "Менеджер";
                    worksheet.Cells[1, 4].Value = "Дата заказа";
                    worksheet.Cells[1, 5].Value = "Сумма";

                    for (int i = 0; i < orders.Count; i++)
                    {
                        worksheet.Cells[i + 2, 1].Value = orders[i].Id;
                        worksheet.Cells[i + 2, 2].Value = orders[i].Client?.ClientName ?? "N/A";
                        worksheet.Cells[i + 2, 3].Value = orders[i].User?.Firstname ?? "N/A";
                        worksheet.Cells[i + 2, 4].Value = orders[i].CreatedAt;
                        worksheet.Cells[i + 2, 5].Value =
                            orders[i].OrderCars.Sum(m => m.Car.Price); // Сумма цен автомобилей
                    }

                    var fileInfo = new FileInfo("Отчет_по_заказам.xlsx");
                    package.SaveAs(fileInfo);

                    MessageBox.Show("Отчет успешно сохранен!");
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