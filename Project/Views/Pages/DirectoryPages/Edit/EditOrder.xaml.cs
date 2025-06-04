using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using Project.Interfaces;
using Project.Models;
using Project.Tools;
using Wpf.Ui.Common;
using MessageBox = System.Windows.MessageBox;

namespace Project.Views.Pages.DirectoryPages.Edit
{
    public partial class EditOrder : IRefresh
    {
        public event Action RefreshRequested;
        private readonly bool _isEditMode;
        private readonly bool _isDeleteMode;
        private readonly ulong _itemId;
        private readonly ulong _currentStatus;
        public ObservableCollection<Car> SelectedOrderCars { get; set; } = new ObservableCollection<Car>();

        // Конструктор для добавления данных
        public EditOrder()
        {
            InitializeComponent();
            Init();
            _isEditMode = false;
            _isDeleteMode = false;
            // Установка значений в форму
            Title = "Добавление данных";
            SaveButton.Content = "Добавить";
            SaveButton.Icon = SymbolRegular.AddCircle24;
            ShowStatus.Visibility = Visibility.Collapsed;
            EditOrdersStatus.Visibility = Visibility.Collapsed;
            EditOrdersStatus.SelectedItem = DbUtils.db.Statuses
                .FirstOrDefault(m => m.Id == 1);
            EditOrdersData.SelectedDate = DateTime.Now;
            EditOrdersUsers.SelectedItem = DbUtils.db.Users
                .FirstOrDefault(m => m.Id == Global.CurrentUser.Id);
        }

        // Конструктор для изменения (удаления) данных
        public EditOrder(Order item, string button) : this()
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            
            Init();
            _itemId = item.Id;

            // Установка значений в форму
            ShowId.Visibility = Visibility.Visible;
            ShowOrdersId.Text = item.Id.ToString();
            EditOrdersClient.SelectedItem =
                DbUtils.db.Clients.FirstOrDefault(m => m.Id == item.ClientId);
            EditOrdersUsers.SelectedItem =
                DbUtils.db.Users.FirstOrDefault(m => m.Id == item.UserId);
            ShowOrdersData.Text = item.CreatedAt?.ToString("dd.MM.yyyy") ?? DateTime.Now.ToString("dd.MM.yyyy");
            EditOrdersData.SelectedDate = item.CreatedAt ?? DateTime.Now;
            EditOrdersPayment.SelectedItem =
                DbUtils.db.Payments.FirstOrDefault(m => m.Id == item.PaymentId);
            EditOrdersDelivery.SelectedItem =
                DbUtils.db.Deliveries.FirstOrDefault(m => m.Id == item.DeliveryId);
            EditOrdersAddress.Text = item.DeliveryAddress;
            // Получение статуса Заказа
            ShowStatus.Visibility = Visibility.Visible;
            EditOrdersStatus.Visibility = Visibility.Visible;
            var latestStatusId = item.OrderStatuses
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefault()?.StatusId;
            _currentStatus = (ulong)(latestStatusId ?? 1);
            EditOrdersStatus.SelectedItem =
                DbUtils.db.OrderStatuses.FirstOrDefault(m => m.Id == _currentStatus);

            // Загрузка связанных автомобилей
            var carsInOrder = DbUtils.db.OrderCars
                .Include(m => m.Car)
                
                .Include(m => m.Car.Color)
                .Include(m => m.Car.Type)
                .Where(moc => moc.OrderId == item.Id)
                .Select(moc => moc.Car)
                .ToList();

            foreach (var car in carsInOrder)
                SelectedOrderCars.Add(car);

            // изменяем диалоговое окно, в зависимости от нажатой кнопки
            if (button == "Change")
            {
                _isEditMode = true;
                Title = "Изменение данных";
                SaveButton.Content = "Изменить";
                SaveButton.Icon = SymbolRegular.EditProhibited28;
                ShowOrdersData.Visibility = Visibility.Collapsed;
                EditOrdersData.Visibility = Visibility.Visible;
            }
            else if (button == "Show")
            {
                _isEditMode = true;
                Title = "Просмотр данных";
                SaveButton.Visibility = Visibility.Collapsed;
                ShowOrdersData.Visibility = Visibility.Visible;
                EditOrdersData.Visibility = Visibility.Collapsed;
            }

            if (button == "Delete")
            {
                _isDeleteMode = true;
                Title = "Удаление данных";
                SaveButton.Content = "Удалить";
                SaveButton.Icon = SymbolRegular.Delete24;
            }
        }

        // Обработка нажатия кнопки "Сохранить"
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var item = (_isEditMode || _isDeleteMode)
                    ? DbUtils.db.Orders.FirstOrDefault(x => x.Id == _itemId)
                    : new Order();

                if (item == null)
                {
                    MessageBox.Show("Данные не найдены.", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Удаление
                if (_isDeleteMode)
                {
                    RemoveCarsAndUnblock(item.Id);
                    DbUtils.db.Orders.Remove(item);
                }
                else
                {
                    if (!IsValidInput())
                        return;

                    // Изменение или добавление
                    UpdateItem(item);

                    if (!_isEditMode)
                    {
                        DbUtils.db.Orders.Add(item);
                    }

                    DbUtils.db.SaveChanges();

                    // Сохранение нового статуса заказа
                    var selectedStatus = (EditOrdersStatus.SelectedItem as Status)?.Id;

                    if (selectedStatus == null)
                    {
                        selectedStatus = 1; // Статус "Создан"
                    }

                    // Удаление авто из заказа, если статус "Отменен"
                    if (selectedStatus == 4)
                    {
                        RemoveCarsAndUnblock(item.Id);
                    }

                    // Сохранение статуса, если он изменился
                    if (_currentStatus != selectedStatus.Value)
                    {
                        var newStatus = new OrderStatus
                        {
                            OrderId = item.Id,
                            StatusId = selectedStatus.Value,
                            CreatedAt = DateTime.Now
                        };

                        DbUtils.db.OrderStatuses.Add(newStatus);
                    }

                    // Сохранение автомобилей в заказе, если он не был Отменен
                    if (selectedStatus != 4)
                    {
                        foreach (var car in SelectedOrderCars)
                        {
                            if (!DbUtils.db.OrderCars.Any(moc =>
                                    moc.OrderId == item.Id && moc.CarId == car.Id))
                            {
                                var newOrderCar = new OrderCar { OrderId = item.Id, CarId = car.Id };
                                DbUtils.db.OrderCars.Add(newOrderCar);
                            }

                            // Устанавливаем флаг Block = номер заказа
                            var carToUpdate = DbUtils.db.Cars.FirstOrDefault(c => c.Id == car.Id);
                            if (carToUpdate != null)
                            {
                                carToUpdate.Block = item.Id;
                            }
                        }
                    }
                }

                DbUtils.db.SaveChanges();
                RefreshRequested?.Invoke();
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения к базе данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Закрытие окна
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // Инициализация данных
        private void Init()
        {
            EditOrdersClient.ItemsSource = DbUtils.db.Clients.Where(x => x.DeletedAt == null && x.ClientStatus).ToList();
            // Только из Отдела продаж
            EditOrdersUsers.ItemsSource = DbUtils.db.Users.Where(x => x.DeletedAt == null && x.DepartmentId == 4).ToList();
            EditOrdersPayment.ItemsSource = DbUtils.db.Payments.Where(x => x.DeletedAt == null).ToList();
            EditOrdersDelivery.ItemsSource = DbUtils.db.Deliveries.Where(x => x.DeletedAt == null).ToList();
            EditOrdersStatus.ItemsSource = DbUtils.db.Statuses.Where(x => x.DeletedAt == null).ToList();

            // Загружаем доступные автомобили (с учетом уже добавленных)
            UpdateAvailableCars();

            DataContext = this;
        }

        // Валидация данных
        private bool IsValidInput()
        {
            if (EditOrdersClient.SelectedItem == null)
            {
                MessageBox.Show("Не выбран клиент", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (EditOrdersUsers.SelectedItem == null)
            {
                MessageBox.Show("Не выбран менеджер", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (EditOrdersPayment.SelectedItem == null)
            {
                MessageBox.Show("Не выбран тип оплаты", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (EditOrdersDelivery.SelectedItem == null)
            {
                MessageBox.Show("Не выбран тип доставки", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(EditOrdersAddress.Text))
            {
                MessageBox.Show("Требуется заполнить адрес доставки.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (EditOrdersStatus.SelectedItem == null)
            {
                MessageBox.Show("Не выбран статус заказа", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        // Обновление данных объекта
        private void UpdateItem(Order item)
        {
            item.ClientId = (EditOrdersClient.SelectedItem as Client)?.Id ?? item.ClientId;
            item.UserId = (EditOrdersUsers.SelectedItem as User)?.Id ?? item.UserId;
            item.CreatedAt = EditOrdersData.SelectedDate.HasValue
                ? (DateTime?)EditOrdersData.SelectedDate.Value
                : DateTime.Now;
            item.PaymentId = (EditOrdersPayment.SelectedItem as Payment)?.Id ?? item.PaymentId;
            item.DeliveryId = (EditOrdersDelivery.SelectedItem as Delivery)?.Id ?? item.DeliveryId;
            item.DeliveryAddress = (item.DeliveryId == 1)
                ? "Москва. Основной склад"
                : EditOrdersAddress.Text.Trim();
        }

        // Фокус на элементе
        private void UiWindow_Loaded(object sender, RoutedEventArgs e)
        {
            EditOrdersClient.Focus();
        }

        // Выбор типа доставки
        private void SelectionDelivery(object sender, SelectionChangedEventArgs e)
        {
            var selectDelivery = EditOrdersDelivery.SelectedItem as Delivery;

            if (selectDelivery != null)
            {
                if (selectDelivery.Id == 1)
                {
                    EditAddressName.Visibility = Visibility.Collapsed;
                    EditAddressData.Visibility = Visibility.Collapsed;
                    EditOrdersAddress.Text = "Москва. Основной склад";
                }
                else
                {
                    EditAddressName.Visibility = Visibility.Visible;
                    EditAddressData.Visibility = Visibility.Visible;
                }
            }
        }

        // Добавить автомобиль в заказ
        private void AddCarButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedCar = AvailableCarsComboBox.SelectedItem as Car;
            if (selectedCar == null)
            {
                MessageBox.Show("Выберите автомобиль для добавления.", "Ошибка", MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (SelectedOrderCars.Any(c => c.Id == selectedCar.Id))
            {
                MessageBox.Show("Этот автомобиль уже добавлен.", "Ошибка", MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            SelectedOrderCars.Add(selectedCar);

            if (_itemId != 0UL)
            {
                var newOrderCar = new OrderCar { OrderId = _itemId, CarId = selectedCar.Id };
                DbUtils.db.OrderCars.Add(newOrderCar);
            }

            UpdateAvailableCars();
        }

        // Удалить автомобиль из заказа
        private void RemoveCarButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedCar = AddedCarsList.SelectedItem as Car;
            if (selectedCar == null)
            {
                MessageBox.Show("Выберите автомобиль для удаления.", "Ошибка", MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            SelectedOrderCars.Remove(selectedCar);

            if (_itemId != 0UL)
            {
                var carToRemove = DbUtils.db.OrderCars
                    .FirstOrDefault(moc => moc.CarId == selectedCar.Id && moc.OrderId == _itemId);

                if (carToRemove != null)
                    DbUtils.db.OrderCars.Remove(carToRemove);

                // Снимаем блокировку автомобиля (Block = false)
                var carToUnblock = DbUtils.db.Cars.FirstOrDefault(c => c.Id == selectedCar.Id);
                if (carToUnblock != null)
                {
                    carToUnblock.Block = 0;
                }
            }

            UpdateAvailableCars();
        }

        // Удаление и разблокировка авто (удаление или отмена заказа)
        private void RemoveCarsAndUnblock(ulong orderId)
        {
            var carsToRemove = DbUtils.db.OrderCars.Where(moc => moc.OrderId == orderId).ToList();

            DbUtils.db.OrderCars.RemoveRange(carsToRemove);

            foreach (var car in carsToRemove)
            {
                var carToUnblock = DbUtils.db.Cars.FirstOrDefault(c => c.Id == car.CarId);
                if (carToUnblock != null)
                {
                    carToUnblock.Block = 0;
                    // DbUtils.db.SaveChanges();
                }
            }

            UpdateAvailableCars();
        }

        // Обновление автомобилей в форме выбора
        private void UpdateAvailableCars()
        {
            // Получаем ID уже добавленных автомобилей
            var selectedCarIds = SelectedOrderCars.Select(c => c.Id).ToList();

            // Фильтруем доступные автомобили
            AvailableCarsComboBox.ItemsSource = DbUtils.db.Cars
                .Where(c => c.DeletedAt == null && (c.Block == 0))
                .Include(m => m.Mark)
                .Include(m => m.Model)
                .Include(c => c.Color)
                .Include(c => c.Type)
                .AsEnumerable()
                .Where(c => !selectedCarIds.Contains(c.Id))
                .ToList();
        }
        
        // Вызов окна для добавления нового клиента
        private void AddClient(object sender, RoutedEventArgs e)
        {
            var addClient = new EditOrdersClient();
            this.Close();
            addClient.ShowDialog();
        }

        // Вызов окна для просмотра авто
        private void ShowCar(object sender, RoutedEventArgs e)
        {
            var selectedCar = AddedCarsList.SelectedItem as Car;

            if (selectedCar != null)
            {
                var showCar = new EditCar(selectedCar, "Show");
                showCar.ShowDialog();
            }
        }
    }
}