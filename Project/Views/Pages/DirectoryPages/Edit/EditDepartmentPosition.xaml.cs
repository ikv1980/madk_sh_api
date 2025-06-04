using System.Windows;
using Project.Interfaces;
using Project.Models;
using Project.Tools;
using Wpf.Ui.Common;
using Wpf.Ui.Controls;
using MessageBox = System.Windows.MessageBox;

namespace Project.Views.Pages.DirectoryPages.Edit
{
    public partial class EditDepartmentPosition : UiWindow, IRefresh
    {
        public event Action RefreshRequested;
        private readonly bool _isEditMode;
        private readonly bool _isDeleteMode;
        private readonly ulong _itemId;

        // Конструктор для добавления данных
        public EditDepartmentPosition()
        {
            InitializeComponent();
            Init();
            _isEditMode = false;
            _isDeleteMode = false;
            Title = "Добавление данных";
            SaveButton.Content = "Добавить";
            SaveButton.Icon = SymbolRegular.AddCircle24;
            EditPositionName.Width = 255;
            EditDepartmentName.Width = 255;
            ButtonAddPosition.Visibility = Visibility.Visible;
            ButtonAddDepartment.Visibility = Visibility.Visible;
        }

        // Конструктор для изменения (удаления) данных
        public EditDepartmentPosition(UserDepartmentPosition item, string button) : this()
        {
            Init();
            _itemId = item.Id;
            EditDepartmentName.SelectedItem =
                DbUtils.db.UserDepartments.FirstOrDefault(m => m.Id == item.DepartmentId);
            EditPositionName.SelectedItem =
                DbUtils.db.UserPositions.FirstOrDefault(m => m.Id == item.PositionId);
            EditPositionName.Width = 300;
            EditDepartmentName.Width = 300;
            ButtonAddPosition.Visibility = Visibility.Collapsed;
            ButtonAddDepartment.Visibility = Visibility.Collapsed;

            // изменяем диалоговое окно, в зависимости от нажатой кнопки
            if (button == "Change")
            {
                _isEditMode = true;
                Title = "Изменение данных";
                SaveButton.Content = "Изменить";
                SaveButton.Icon = SymbolRegular.EditProhibited28;
            }
            else if (button == "Show")
            {
                _isEditMode = true;
                Title = "Просмотр данных";
                SaveButton.Visibility = Visibility.Collapsed;
            }
            else if (button == "Delete")
            {
                _isDeleteMode = true;
                Title = "Удаление данных";
                SaveButton.Content = "Удалить";
                DeleteTextBlock.Visibility = Visibility.Visible;
                SaveButton.Icon = SymbolRegular.Delete24;
            }
        }

        // Обработка нажатия кнопки "Сохранить"
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var item = (_isEditMode || _isDeleteMode)
                    ? DbUtils.db.UserDepartmentPositions.FirstOrDefault(x => x.Id == _itemId)
                    : new UserDepartmentPosition();

                if (item == null)
                {
                    MessageBox.Show("Данные не найдены.", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Удаление
                if (_isDeleteMode)
                {
                    DbUtils.db.UserDepartmentPositions.Remove(item);
                }
                else
                {
                    if (!IsValidInput())
                        return;

                    // Изменение или добавление
                    UpdateItem(item);

                    if (!_isEditMode)
                    {
                        DbUtils.db.UserDepartmentPositions.Add(item);
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
            this.Close();
        }

        // Инициализация данных для списков
        private void Init()
        {
            EditDepartmentName.ItemsSource = DbUtils.db.UserDepartments.Where(x => x.DeletedAt == null).ToList();
            EditPositionName.ItemsSource = DbUtils.db.UserPositions.Where(x => x.DeletedAt == null).ToList();
        }

        // Валидация данных
        private bool IsValidInput()
        {
            if (EditDepartmentName.SelectedItem == null)
            {
                MessageBox.Show("Не выбран отдел", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (EditPositionName.SelectedItem == null)
            {
                MessageBox.Show("Не выбрана должность", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (DbUtils.db.UserDepartmentPositions.Any(x =>
                    x.DepartmentId == (ulong)EditDepartmentName.SelectedValue &&
                    x.PositionId == (ulong)EditPositionName.SelectedValue &&
                    x.Id != _itemId))
            {
                MessageBox.Show("Такая запись уже существует в базе данных.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        // Обновление данных объекта
        private void UpdateItem(UserDepartmentPosition item)
        {
            item.DepartmentId = (EditDepartmentName.SelectedItem as UserDepartment)?.Id ?? item.DepartmentId;
            item.PositionId = (EditPositionName.SelectedItem as UserPosition)?.Id ?? item.PositionId;
        }

        // События после загрузки окна
        private void UiWindow_Loaded(object sender, RoutedEventArgs e)
        {
            EditPositionName.Focus();
        }

        private void AddPosition_Click(object sender, RoutedEventArgs e)
        {
            var addPosition = new EditPosition();
            this.Close();
            addPosition.ShowDialog();
        }

        private void AddDepartment_Click(object sender, RoutedEventArgs e)
        {
            var addDepartment = new EditDepartment();
            this.Close();
            addDepartment.ShowDialog();
        }
    }
}