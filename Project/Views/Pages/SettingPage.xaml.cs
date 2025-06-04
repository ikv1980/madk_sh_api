using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using Project.Models;
using Project.Views.Pages.DirectoryPages.Edit;
using Page = System.Windows.Controls.Page;

namespace Project.Views.Pages
{
    public partial class SettingPage : Page
    {
        private Db _dbContext;
        private bool _isEditMode = false;
        private Dictionary<string, object> _originalValues = new();

        public SettingPage()
        {
            InitializeComponent();
            _dbContext = new Db();
            InitializeDataAsync();
            this.Unloaded += SettingPage_Unloaded;
        }

        // Инициализация данных страницы
        private async void InitializeDataAsync()
        {
            await LoadUsersAsync();
        }

        // Загрузка пользователей из базы данных
        private async Task LoadUsersAsync()
        {
            UserPermissionsTable.ItemsSource = await _dbContext.Users
                .Include(u => u.Department)
                .Include(u => u.Position)
                .ToListAsync();
        }
        
        // Изменение прав доступа пользователя
        private async void EditPermissions_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { Tag: User user })
            {
                var permissionWindow = new EditPermission(user);
                if (permissionWindow.ShowDialog() == true)
                {
                    await LoadUsersAsync();
                }
            }
        }

        // Сохранение изменений
        private async void SaveChangesAsync()
        {
            if (_dbContext.ChangeTracker.HasChanges())
            {
                await _dbContext.SaveChangesAsync();
                MessageBox.Show("Изменения сохранены", "Сохранение", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        
        // Освобождение ресурсов при выгрузке страницы
        private void SettingPage_Unloaded(object sender, RoutedEventArgs e)
        {
            _dbContext.Dispose();
        }
    }
}