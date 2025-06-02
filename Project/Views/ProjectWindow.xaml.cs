using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ModernWpf.Controls;
using Project.Models;
using Project.Tools;
using Project.Views.Pages;
using Page = System.Windows.Controls.Page;

namespace Project.Views
{
    public partial class ProjectWindow : Window
    {
        public ProjectWindow(User user)
        {
            InitializeComponent();
            Global.CurrentUser = user;
            Global.ParsePermissions(user);
            this.Loaded += change_Screeen;
            MainTabControl.SelectedIndex = 1;
            SecondTabControl.SelectedIndex = 0;
            MainContent.Content = new UserPage();
            SetAccess();
        }

        // Доступ к вкладкам и справочникам пользователя
        private void SetAccess()
        {
            if (Global.ParsedPermissions?.Tabs == null || Global.ParsedPermissions.Tabs.Count == 0)
            {
                MessageBox.Show("Разрешения пользователя не определены. Доступ ограничен.", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Доступ к вкладкам
            foreach (var tabPermission in Global.ParsedPermissions.Tabs)
            {
                var visibility = tabPermission.Permissions.Read ? Visibility.Visible : Visibility.Collapsed;

                switch (tabPermission.Name.ToLower())
                {
                    case "user":
                        UserPage.Visibility = visibility;
                        break;
                    case "order":
                        OrderPage.Visibility = visibility;
                        break;
                    case "report":
                        ReportPage.Visibility = visibility;
                        break;
                    case "setting":
                        SettingTab.Visibility = visibility;
                        break;
                    case "dict":
                        Directoryes.Visibility = visibility;
                        break;
                }
            }

            // Доступ к справочникам
            foreach (var directoryesPermission in Global.ParsedPermissions.Directoryes)
            {
                var visibility = directoryesPermission.Permissions.Read ? Visibility.Visible : Visibility.Collapsed;

                switch (directoryesPermission.Name.ToLower())
                {
                    // для Заказов
                    case "order":
                        OrdersClientButton.Visibility = visibility;
                        break;
                    case "ordersclient":
                        OrdersClientButton.Visibility = visibility;
                        break;
                    case "ordersdelivery":
                        OrdersDeliveryButton.Visibility = visibility;
                        break;
                    case "orderspayment":
                        OrdersPaymentButton.Visibility = visibility;
                        break;
                    case "ordersstatus":
                        OrdersStatusButton.Visibility = visibility;
                        break;
                    // для Автомобилей
                    case "car":
                        CarsButton.Visibility = visibility;
                        break;
                    case "carscountry":
                        CarsCountryButton.Visibility = visibility;
                        break;
                    case "carsmark":
                        CarsMarkButton.Visibility = visibility;
                        break;
                    case "carsmodel":
                        CarsModelButton.Visibility = visibility;
                        break;
                    case "carstype":
                        CarsTypeButton.Visibility = visibility;
                        break;
                    case "carscolor":
                        CarsColorButton.Visibility = visibility;
                        break;
                    case "mmmarkmodel":
                        MmMarkModelButton.Visibility = visibility;
                        break;
                    // для Пользователей
                    case "user":
                        UsersButton.Visibility = visibility;
                        break;
                    case "usersdepartment":
                        UsersDepartmentButton.Visibility = visibility;
                        break;
                    case "usersfunction":
                        UsersFunctionButton.Visibility = visibility;
                        break;
                    case "usersstatus":
                        UsersStatusButton.Visibility = visibility;
                        break;
                    case "mmdepartmentfunction":
                        MmDepartmentFunctionButton.Visibility = visibility;
                        break;
                    // по умолчанию
                    default:
                        Console.WriteLine($"Unknown directory: {directoryesPermission.Name}");
                        break;
                }
            }

            // Обновление видимости всех Expander
            UpdateExpanderVisibility(OrdersButtonPanel, OrdersExpander);
            UpdateExpanderVisibility(CarsButtonPanel, CarsExpander);
            UpdateExpanderVisibility(UsersButtonPanel, UsersExpander);
        }

        // Скрытие панелей с кнопками
        private void UpdateExpanderVisibility(StackPanel buttonPanel, Expander expander)
        {
            bool allButtonsCollapsed = true;

            foreach (var child in buttonPanel.Children)
            {
                if (child is Button button && button.Visibility == Visibility.Visible)
                {
                    allButtonsCollapsed = false;
                    break;
                }
            }

            expander.Visibility = allButtonsCollapsed ? Visibility.Collapsed : Visibility.Visible;
        }

        // Выбор вкладки
        private void TabControl_Select(object sender, SelectionChangedEventArgs e)
        {
            if (sender is TabControl tabControl)
            {
                if (tabControl == MainTabControl)
                {
                    SecondTabControl.SelectedIndex = 0;
                }
            }

            TabItem selectedItem = MainTabControl.SelectedItem as TabItem;
            if (selectedItem != null)
            {
                // Проверяем, является ли Tag типом страницы
                if (selectedItem.Tag is string pageTypeString)
                {
                    Type pageType = Type.GetType(pageTypeString);
                    if (pageType != null && typeof(Page).IsAssignableFrom(pageType))
                    {
                        // Создаем страницу и устанавливаем ее как содержимое TabItem
                        var page = (Page)Activator.CreateInstance(pageType);
                        MainContent.Content = page; // Устанавливаем содержимое в Frame
                    }
                }
                else
                {
                    this.Close();
                }
            }
        }

        // Выбор справочника
        private void NavigationView_SelectionChanged(ModernWpf.Controls.NavigationView sender,
            ModernWpf.Controls.NavigationViewSelectionChangedEventArgs args)
        {
            NavigationViewItem item = args.SelectedItem as NavigationViewItem;
            if (item.Tag is Type pageType && typeof(System.Windows.Controls.Page).IsAssignableFrom(pageType))
            {
                MainContent.Content = (System.Windows.Controls.Page)Activator.CreateInstance(pageType);
            }
            else if (item.Tag != null)
            {
                this.Close();
            }
        }

        // Нажатие на вкладку "Справочники"
        private void Directoryes_MouseUp(object sender, MouseButtonEventArgs e)
        {
            SubMenuPopup.IsOpen = !SubMenuPopup.IsOpen;
            MainTabControl.SelectedIndex = -1;
            SecondTabControl.SelectedIndex = 1;
            // Сворачивание списков
            OrdersExpander.IsExpanded = false;
            CarsExpander.IsExpanded = false;
            UsersExpander.IsExpanded = false;
        }

        private void SubMenuButton_Click(object sender, RoutedEventArgs e)
        {
            SubMenuPopup.IsOpen = !SubMenuPopup.IsOpen;
            var button = sender as Button;
            if (button != null)
            {
                var pageType = button.Tag as Type;
                if (pageType != null)
                {
                    MainContent.Content = (System.Windows.Controls.Page)Activator.CreateInstance(pageType);
                }
            }
        }

        // Изменение размера рабочего экрана
        private void change_Screeen(object sender, RoutedEventArgs e)
        {
            if (SystemParameters.PrimaryScreenHeight > 1000)
            {
                this.Width = 1250;
                this.Height = 960;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }
    }
}