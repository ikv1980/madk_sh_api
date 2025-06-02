using Project.Tools;
using System.Windows;

namespace Project.Views
{
    public partial class ConnectionDB : Window
    {
        public ConnectionDB()
        {
            InitializeComponent();
        }

        private async void LoadDb()
        {
            try
            {
                bool canConnect = await Task.Run(() => DbUtils.db.Database.CanConnect());

                if (canConnect)
                {
                    new AuthWindow().Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Не удалось подключиться к базе данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения к БД\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(0);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDb();
        }
    }
}