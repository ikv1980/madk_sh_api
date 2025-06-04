using Project.Tools;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using Project.Interfaces;
using Project.Models;

namespace Project.ViewModels
{
    internal class OrderPageViewModel : SomePagesViewModel<Order>
    {
        public ObservableCollection<Order> Orders { get; set; }
        public ObservableCollection<Car> SelectedOrderCars { get; set; }
        public ObservableCollection<OrderStatus> SelectedOrderStatuses { get; set; }

        private Order _selectedOrder;

        public Visibility OrderDetailsVisibility => SelectedOrder != null ? Visibility.Visible : Visibility.Collapsed;

        public Order SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                if (_selectedOrder != value)
                {
                    _selectedOrder = value;
                    OnPropertyChanged(nameof(SelectedOrder));
                    LoadOrderDetails();
                }
            }
        }

        public OrderPageViewModel()
        {
            SelectedOrderCars = new ObservableCollection<Car>();
            SelectedOrderStatuses = new ObservableCollection<OrderStatus>();

            // Инициализация данных
            InitializeOrdersAsync();
        }

        private async void InitializeOrdersAsync()
        {
            var orders = await DbUtils.GetTablePagedValuesWithIncludes<Order>(1, 20);
            Orders = new ObservableCollection<Order>(orders);
        }

        protected override async Task Refresh()
        {
            await base.Refresh(); // Загружаем базовые данные

            var orderValues = TableValue.Cast<Order>().Where(order =>
            {
                var lastStatus = order.OrderStatuses
                    .OrderByDescending(status => status.CreatedAt)
                    .FirstOrDefault();

                if (lastStatus != null)
                {
                    if (lastStatus.StatusId == 4)
                        return false;

                    if (lastStatus?.CreatedAt != null)
                    {
                        TimeSpan timePassed = DateTime.Now - lastStatus.CreatedAt.Value;
                        double daysSinceCompleted = timePassed.TotalDays;

                        if (daysSinceCompleted >= 5)
                            return false;
                    }
                }

                return true;
            }).ToList();

            TableValue = new ObservableCollection<Order>(orderValues);
        }

        private void LoadOrderDetails()
        {
            // Если заказ выбран, загрузим связанные данные
            if (SelectedOrder != null)
            {
                SelectedOrderCars.Clear();
                SelectedOrderStatuses.Clear();

                // Загрузка автомобилей
                var cars = DbUtils.db.OrderCars
                    .Include(m => m.Car).ThenInclude(mmc => mmc.Mark)
                    .Include(m => m.Car).ThenInclude(mmc => mmc.Model)
                    .Include(m => m.Car).ThenInclude(c => c.Color)
                    .Where(m => m.OrderId == SelectedOrder.Id)
                    .Select(m => m.Car)
                    .ToList();

                foreach (var car in cars)
                    SelectedOrderCars.Add(car);

                // Загрузка статусов
                var statuses = DbUtils.db.OrderStatuses
                    .Include(s => s.Status)
                    .Where(s => s.OrderId == SelectedOrder.Id)
                    .OrderBy(s => s.CreatedAt)
                    .ToList();

                foreach (var status in statuses)
                    SelectedOrderStatuses.Add(status);

                // Уведомление об изменении видимости
                OnPropertyChanged(nameof(OrderDetailsVisibility));
            }
        }
    }
}