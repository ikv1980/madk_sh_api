using System.Windows;
using Project.Interfaces;
using Project.Models;
using Project.Tools;
using Wpf.Ui.Common;
using Wpf.Ui.Controls;
using MessageBox = System.Windows.MessageBox;

namespace Project.Views.Pages.DirectoryPages.Edit
{
    public partial class EditDepartment : UiWindow, IRefresh
    {
        public event Action RefreshRequested;
        private readonly bool _isEditMode;
        private readonly bool _isDeleteMode;
        private readonly ulong _itemId;

        // Конструктор для добавления данных
        public EditDepartment()
        {
            InitializeComponent();
            _isEditMode = false;
            _isDeleteMode = false;
            Title = "Добавление данных";
            SaveButton.Content = "Добавить";
            SaveButton.Icon = SymbolRegular.AddCircle24;
        }

        // Конструктор для изменения (удаления) данных
        public EditDepartment(UserDepartment item, string button) : this()
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            _itemId = item.Id;
            EditDepartmentName.Text = item.DepartmentName;
            EditDescriptionName.Text = item.DepartmentDescription;

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
                SaveButton.Icon = SymbolRegular.Delete24;
                DeleteTextBlock.Visibility = Visibility.Visible;
            }
        }

        // Обработка нажатия кнопки "Сохранить"
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var item = (_isEditMode || _isDeleteMode)
                    ? DbUtils.db.UserDepartments.FirstOrDefault(x => x.Id == _itemId)
                    : new UserDepartment();

                if (item == null)
                {
                    MessageBox.Show("Данные не найдены.", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Удаление
                if (_isDeleteMode)
                {
                    item.DeletedAt = DateTime.Now; //DbUtils.db.UsersDepartments.Remove(item);   
                }
                else
                {
                    if (!IsValidInput())
                        return;

                    // Изменение или добавление
                    item.DepartmentName = EditDepartmentName.Text.Trim();
                    item.DepartmentDescription = EditDescriptionName.Text.Trim();

                    if (!_isEditMode)
                    {
                        DbUtils.db.UserDepartments.Add(item);
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

        // Валидация данных
        private bool IsValidInput()
        {
            var item = EditDepartmentName.Text.Trim().ToLower();
            var description = EditDescriptionName.Text.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(item))
            {
                MessageBox.Show("Поле [Название отдела] не должно быть пустым.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            
            if (string.IsNullOrWhiteSpace(description))
            {
                MessageBox.Show("Поле [Описание] не должно быть пустым.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (DbUtils.db.UserDepartments.Any(x => x.DepartmentName == item && x.Id != _itemId))
            {
                MessageBox.Show($"Запись '{EditDepartmentName.Text}' уже существует в базе.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        // События после загрузки окна
        private void UiWindow_Loaded(object sender, RoutedEventArgs e)
        {
            EditDepartmentName.Focus();
        }
    }
}