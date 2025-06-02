using System.Globalization;
using Project.Models;
using System.Windows;
using System.Windows.Controls;
using Project.Tools;
using Microsoft.EntityFrameworkCore;

namespace Project.Views.Pages
{
    public partial class UserPage : Page
    {
        private readonly UserViewModel _viewModel;
        private readonly User _user;
        private readonly ValidateField _validator;
        private readonly Helpers _helper;
        private bool _showButton;
        private bool _flagWriter;

        public UserPage()
        {
            InitializeComponent();
            _user = Global.CurrentUser;
            _viewModel = new UserViewModel(_user);
            DataContext = _viewModel;
            _validator = new ValidateField();
            _helper = new Helpers();
            _flagWriter = Global.GetWritePermissionForTab("user");

            // Отслеживание изменений
            NewPasswordBox.PasswordChanged += OnFieldChanged;
            NewEmailTextBox.TextChanged += OnFieldChanged;
            NewPhoneTextBox.TextChanged += OnFieldChanged;
        }

        public class UserViewModel
        {
            public string FullName { get; }
            public string Login { get; }
            public string Password { get; set; }
            public string UsersEmail { get; set; }
            public string Phone { get; set; }
            public string Birthday { get; }
            public string DepartmentId { get; }
            public string PositionId { get; }
            public string StatusId { get; }
            public string StartWork { get; }

            public UserViewModel(User user)
            {
                FullName = $"{user.Firstname} {user.Patronymic} {user.Surname}";
                Login = user.Login;
                UsersEmail = user.Email;
                Phone = user.Phone;
                Birthday = user.Birthday?.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture) ??
                                string.Empty;
                StartWork = user.StartWork?.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture) ??
                                 string.Empty;
                StatusId = $"{user.Status.StatusName} (с {user.StatusAt:dd.MM.yyyy})";
                DepartmentId = user.Department.DepartmentName;
                PositionId = user.Position.PositionName;
            }
        }

        // Отображение кнопки при изменениях в полях
        private void OnFieldChanged(object sender, RoutedEventArgs e)
        {
            _showButton = _flagWriter && (!string.IsNullOrWhiteSpace(NewPasswordBox.Password) ||
                                          NewEmailTextBox.Text != _viewModel.UsersEmail ||
                                          NewPhoneTextBox.Text != _viewModel.Phone);
            UpdateButton.Visibility = _showButton ? Visibility.Visible : Visibility.Hidden;
        }

        // Сохранение изменений
        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs())
            {
                return;
            }

            // Добавление новых данных
            if (!string.IsNullOrWhiteSpace(NewPasswordBox.Password))
                _viewModel.Password = NewPasswordBox.Password;

            if (NewEmailTextBox.Text != _viewModel.UsersEmail)
                _viewModel.UsersEmail = NewEmailTextBox.Text;

            if (NewPhoneTextBox.Text != _viewModel.Phone)
                _viewModel.Phone = NewPhoneTextBox.Text;

            try
            {
                var currentUser =
                    await DbUtils.db.Users.SingleOrDefaultAsync(u => u.Login == _viewModel.Login);

                if (currentUser != null)
                {
                    if (!string.IsNullOrWhiteSpace(_viewModel.Password))
                    {
                        currentUser.Password = _helper.HashPassword(_viewModel.Password);
                    }

                    currentUser.Email = _viewModel.UsersEmail;
                    currentUser.Phone = _viewModel.Phone;

                    await DbUtils.db.SaveChangesAsync();

                    MessageBox.Show("Данные успешно обновлены.", "Сохранение данных", MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    UpdateButton.Visibility = Visibility.Hidden;
                    _showButton = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления данных: {ex.Message}", "Ошибка", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // Валидация полей
        private bool ValidateInputs()
        {
            if (!string.IsNullOrWhiteSpace(NewPasswordBox.Password) &&
                !_validator.IsValid(NewPasswordBox.Password, "password"))
            {
                MessageBox.Show("Пароль должен содержать не менее 6 символов.", "Ошибка данных", MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return false;
            }

            if (!_validator.IsValid(NewEmailTextBox.Text, "email"))
            {
                MessageBox.Show("Некорректный e-mail.", "Ошибка данных", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!_validator.IsValid(NewPhoneTextBox.Text, "phone"))
            {
                MessageBox.Show("Некорректный телефон. В номере телефона допускаются цифры и знак +.", "Ошибка данных",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }
    }
}