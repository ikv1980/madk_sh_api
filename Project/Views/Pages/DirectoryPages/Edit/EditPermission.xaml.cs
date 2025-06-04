using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using Project.Models;
using Project.Tools;
using Wpf.Ui.Controls;
using MessageBox = System.Windows.MessageBox;

namespace Project.Views.Pages.DirectoryPages.Edit
{
    public partial class EditPermission : UiWindow
    {
        private User _user;
        private UserPermissions _userPermissions;

        public EditPermission(User user)
        {
            InitializeComponent();
            _user = user;
            LoadPermissions();
            Title = "Права доступа: " + _user.Surname + " " + _user.Firstname + " " + _user.Patronymic;
            if (!Global.GetWritePermissionForTab("setting"))
            {
                UpdateButton.Visibility = Visibility.Hidden;
            }
        }

        private void LoadPermissions()
        {
            _userPermissions = Global.ParsePermissions(_user);
            
            // Отображение вкладок
            foreach (var tab in _userPermissions.Tabs)
            {
                var stackPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(10, 0, 0, 0),
                };
                var textBlock = new TextBlock
                {
                    Text = tab.RusName, 
                    Margin = new Thickness(5, 0, 5, 0),
                    FontSize = 16, 
                    Width = 170,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                var checkBoxRead = new CheckBox { Content = $"чтение", IsChecked = tab.Permissions.Read };
                var checkBoxWrite = new CheckBox { Content = $"запись", IsChecked = tab.Permissions.Write };
                stackPanel.Children.Add(textBlock);
                stackPanel.Children.Add(checkBoxRead);
                stackPanel.Children.Add(checkBoxWrite);
                TabsList.Items.Add(stackPanel);
            }

            // Отображение справочников
            foreach (var directory in _userPermissions.Directories)
            {
                var stackPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(10, 0, 0, 0),
                };
                var textBlock = new TextBlock
                {
                    Text = directory.RusName, 
                    Margin = new Thickness(5, 0, 5, 0),
                    FontSize = 16, 
                    Width = 170,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                var checkBoxRead = new CheckBox { Content = $"чтение", IsChecked = directory.Permissions.Read };
                var checkBoxWrite = new CheckBox { Content = $"запись", IsChecked = directory.Permissions.Write };
                stackPanel.Children.Add(textBlock);
                stackPanel.Children.Add(checkBoxRead);
                stackPanel.Children.Add(checkBoxWrite);
                DirectoriesList.Items.Add(stackPanel);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Получаем текущие права из интерфейса
            var tabs = _userPermissions.Tabs;
            var directories = _userPermissions.Directories;

            // Обновляем права на основе чекбоксов
            for (int i = 0; i < TabsList.Items.Count; i++)
            {
                var stackPanel = (StackPanel)TabsList.Items[i];
                var checkBoxRead = (CheckBox)stackPanel.Children[1];
                var checkBoxWrite = (CheckBox)stackPanel.Children[2];

                tabs[i].Permissions.Read = checkBoxRead.IsChecked == true;
                tabs[i].Permissions.Write = checkBoxWrite.IsChecked == true;
            }

            for (int i = 0; i < DirectoriesList.Items.Count; i++)
            {
                var stackPanel = (StackPanel)DirectoriesList.Items[i];
                var checkBoxRead = (CheckBox)stackPanel.Children[1];
                var checkBoxWrite = (CheckBox)stackPanel.Children[2];

                directories[i].Permissions.Read = checkBoxRead.IsChecked == true;
                directories[i].Permissions.Write = checkBoxWrite.IsChecked == true;
            }

            // Сохранение изменений в базе данных
            try
            {
                using (var context = new Db())
                {
                    // Найдите пользователя в базе данных
                    var userToUpdate = context.Users.Find(_user.Id);
                    if (userToUpdate != null)
                    {
                        userToUpdate.Permissions = JsonSerializer.Serialize(_userPermissions);
                        context.SaveChanges();
                    }
                }

                MessageBox.Show("Права успешно обновленны!", "Успех", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
