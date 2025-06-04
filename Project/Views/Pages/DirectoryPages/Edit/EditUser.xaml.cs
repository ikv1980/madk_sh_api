using System.Windows;
using System.Windows.Controls;
using Project.Interfaces;
using Project.Models;
using Project.Tools;
using Wpf.Ui.Common;
using MessageBox = System.Windows.MessageBox;

namespace Project.Views.Pages.DirectoryPages.Edit
{
    public partial class EditUser : IRefresh
    {
        public event Action RefreshRequested;
        private readonly bool _isEditMode;
        private readonly bool _isDeleteMode;
        private readonly ulong _itemId;
        private readonly string _oldPassword;
        private readonly ValidateField _validator;

        // Конструктор для добавления данных
        public EditUser()
        {
            InitializeComponent();
            Init();
            _isEditMode = false;
            _isDeleteMode = false;
            Title = "Добавление данных";
            SaveButton.Content = "Добавить";
            SaveButton.Icon = SymbolRegular.AddCircle24;
            ShowUsersLogin.Visibility = Visibility.Collapsed;
            EditUsersLogin.Visibility = Visibility.Visible;
            _validator = new ValidateField();
        }

        // Конструктор для изменения (удаления) данных
        public EditUser(User item, string button) : this()
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            
            Init();
            _oldPassword = item.Password;
            _itemId = item.Id;

            // Установка значений в форму
            EditUsersSurname.Text = item.Surname;
            EditUsersName.Text = item.Firstname;
            EditUsersPatronymic.Text = item.Patronymic;
            EditUsersBirthday.SelectedDate = item.Birthday.HasValue
                ? item.Birthday.Value.ToDateTime(TimeOnly.MinValue)
                : (DateTime?)null;
            EditUsersPhone.Text = item.Phone;
            EditUsersMail.Text = item.Email;
            EditUsersDepartment.SelectedItem = DbUtils.db.UserDepartments.FirstOrDefault(m => m.Id == item.DepartmentId);
            EditUsersPosition.SelectedItem = DbUtils.db.UserPositions.FirstOrDefault(m => m.Id == item.PositionId);
            EditUsersStatus.SelectedItem = DbUtils.db.UserStatuses.FirstOrDefault(m => m.Id == item.StatusId);
            EditUsersStartWork.SelectedDate = item.StartWork.HasValue
                ? item.StartWork.Value.ToDateTime(TimeOnly.MinValue)
                : (DateTime?)null;
            EditUsersStatusChange.SelectedDate = item.StatusAt.HasValue
                ? item.StatusAt.Value.ToDateTime(TimeOnly.MinValue)
                : (DateTime?)null;
            EditUsersLogin.Text = item.Login;
            ShowUsersLogin.Text = item.Login;

            // изменяем диалоговое окно, в зависимости от нажатой кнопки
            if (button == "Change")
            {
                _isEditMode = true;
                Title = "Изменение данных";
                SaveButton.Content = "Изменить";
                SaveButton.Icon = SymbolRegular.EditProhibited28;
                ShowUsersLogin.Visibility = Visibility.Visible;
                EditUsersLogin.Visibility = Visibility.Collapsed;
            }
            else if (button == "Show")
            {
                _isEditMode = true;
                Title = "Просмотр данных";
                SaveButton.Visibility = Visibility.Collapsed;
                ShowUsersLogin.Visibility = Visibility.Visible;
                EditUsersLogin.Visibility = Visibility.Collapsed;
            }
            if (button == "Delete")
            {
                _isDeleteMode = true;
                Title = "Удаление данных";
                SaveButton.Content = "Удалить";
                SaveButton.Icon = SymbolRegular.Delete24;
                EditUsersLogin.Visibility = Visibility.Collapsed;
                DeleteTextBlock.Visibility = Visibility.Visible;
            }
        }

        // Обработка нажатия кнопки "Сохранить"
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var item = (_isEditMode || _isDeleteMode)
                    ? DbUtils.db.Users.FirstOrDefault(x => x.Id == _itemId)
                    : new User();

                if (item == null)
                {
                    MessageBox.Show("Данные не найдены.", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Удаление
                if (_isDeleteMode)
                {
                    // Запрет удаления самого себя
                    if (Global.CurrentUser.Login == item.Login)
                    {
                        MessageBox.Show("Нельзя удалить самого себя.", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        item.DeletedAt = DateTime.Now; //DbUtils.db.Users.Remove(item);  
                    }
                }
                else
                {
                    if (!IsValidInput())
                        return;
                }

                // Изменение
                if (_isEditMode)
                    UpdateItem(item);

                // Добавление
                if (!_isEditMode && !_isDeleteMode)
                {
                    item.Login = EditUsersLogin.Text.Trim();
                    UpdateItem(item);
                    DbUtils.db.Users.Add(item);
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

        // Инициализация данных для списков
        private void Init()
        {
            EditUsersDepartment.ItemsSource = DbUtils.db.UserDepartments.Where(x => x.DeletedAt == null).ToList();
            EditUsersPosition.ItemsSource = DbUtils.db.UserPositions.Where(x => x.DeletedAt == null).ToList();
            EditUsersStatus.ItemsSource = DbUtils.db.UserStatuses.Where(x => x.DeletedAt == null).ToList();
        }

        // Валидация данных
        private bool IsValidInput()
        {
            if (string.IsNullOrWhiteSpace(EditUsersName.Text) || string.IsNullOrWhiteSpace(EditUsersSurname.Text))
            {
                MessageBox.Show("Требуется заполнить Имя и Фамилию", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(EditUsersBirthday.Text))
            {
                MessageBox.Show("Требуется заполнить дату рождения", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!_validator.IsValid(EditUsersPhone.Text, "phone"))
            {
                MessageBox.Show("Некорректный телефон. В номере телефона допускаются цифры и знак +.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!_validator.IsValid(EditUsersMail.Text, "email"))
            {
                MessageBox.Show("Некорректный e-mail.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!_isEditMode && !_isDeleteMode)
            {
                var userLogin = EditUsersLogin.Text.Trim();

                if (string.IsNullOrWhiteSpace(userLogin))
                {
                    MessageBox.Show("Логин клиента не может быть пустым.", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                if (DbUtils.db.Users.Any(x => x.Login == userLogin))
                {
                    MessageBox.Show("Клиент с таким логином уже существует.", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                if (!_validator.IsValid(EditUsersPassword.Password, "password"))
                {
                    MessageBox.Show("Пароль должен содержать не менее 6 символов.", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }

            if (!string.IsNullOrWhiteSpace(EditUsersPassword.Password.Trim()) &&
                !_validator.IsValid(EditUsersPassword.Password, "password"))
            {
                MessageBox.Show("Пароль должен содержать не менее 6 символов.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        // Обновление данных объекта
        private void UpdateItem(User item)
        {
            item.Surname = EditUsersSurname.Text.Trim();
            item.Firstname = EditUsersName.Text.Trim();
            item.Patronymic = EditUsersPatronymic.Text.Trim();
            item.Birthday = EditUsersBirthday.SelectedDate.HasValue
                ? DateOnly.FromDateTime(EditUsersBirthday.SelectedDate.Value)
                : (DateOnly?)null;
            item.Phone = EditUsersPhone.Text.Trim();
            item.Email = EditUsersMail.Text.Trim();
            item.DepartmentId = (EditUsersDepartment.SelectedItem as UserDepartment)?.Id ?? item.DepartmentId;
            item.PositionId = (EditUsersPosition.SelectedItem as UserPosition)?.Id ?? item.PositionId;
            item.StartWork = EditUsersStartWork.SelectedDate.HasValue
                ? DateOnly.FromDateTime(EditUsersStartWork.SelectedDate.Value)
                : DateOnly.FromDateTime(DateTime.Now);
            item.StatusId = (EditUsersStatus.SelectedItem as UserStatus)?.Id ?? item.StatusId;
            item.StatusAt = EditUsersStatusChange.SelectedDate.HasValue
                ? DateOnly.FromDateTime(EditUsersStatusChange.SelectedDate.Value)
                : DateOnly.FromDateTime(DateTime.Now);

            if (!string.IsNullOrWhiteSpace(EditUsersPassword.Password))
            {
                Helpers helper = new Helpers();
                item.Password = helper.HashPassword(EditUsersPassword.Password);
            }
            else
            {
                item.Password = _oldPassword;
            }
        }

        // Фокус на элементе
        private void UiWindow_Loaded(object sender, RoutedEventArgs e)
        {
            EditUsersSurname.Focus();
        }

        private void SelectionDepartment(object sender, SelectionChangedEventArgs e)
        {
            EditUsersPosition.IsEnabled = true;
            var selectDepartment = EditUsersDepartment.SelectedItem as UserDepartment;
            if (selectDepartment != null)
            {
                EditUsersPosition.ItemsSource = DbUtils.db.UserDepartmentPositions
                    .Where(x => x.DepartmentId == selectDepartment.Id)
                    .Select(x => x.Position)
                    .ToList();
            }
            else
            {
                EditUsersPosition.ItemsSource = null;
            }
        }
    }
}