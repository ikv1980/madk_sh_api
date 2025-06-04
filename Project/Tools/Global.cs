using System.Text.Json;
using System.Windows;
using Project.Models;

namespace Project.Tools
{
    internal class Global
    {
        public static User CurrentUser { get; set; }
        public static UserPermissions CurrentPermissions { get; set; }

        // Разбор JSON с возвратом UserPermissions и использованием прав по умолчанию при необходимости
        public static UserPermissions ParsePermissions(User user)
        {
            UserPermissions permissions;

            if (user == null)
            {
                MessageBox.Show("Пользователь не указан. Будут применены права по умолчанию.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                permissions = GetDefaultPermissions();
                return permissions;
            }

            if (!string.IsNullOrWhiteSpace(user.Permissions))
            {
                try
                {
                    permissions = JsonSerializer.Deserialize<UserPermissions>(user.Permissions);

                    // Проверка на наличие вкладок
                    if (permissions?.Tabs == null || permissions.Tabs.Count == 0)
                    {
                        MessageBox.Show("JSON разобран, но вкладки отсутствуют.\nБудут применены права по умолчанию.", "Отладка",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                    }

                    // Проверка на наличие справочников
                    if (permissions?.Directories == null || permissions.Directories.Count == 0)
                    {
                        MessageBox.Show("JSON разобран, но справочники отсутствуют.\nБудут применены права по умолчанию.", "Отладка",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка парсинга JSON: {ex.Message}\nБудут применены права по умолчанию.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    permissions = GetDefaultPermissions();
                }
            }
            else
            {
                MessageBox.Show("Permissions пусто или отсутствует. Будут применены права по умолчанию.", "Отладка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                permissions = GetDefaultPermissions();
            }
            
            return permissions;
        }

        // Получение прав по умолчанию
        public static UserPermissions GetDefaultPermissions()
        {
            return JsonSerializer.Deserialize<UserPermissions>(DefaultPermissions.User);
        }

        // Проверка прав на запись для вкладки
        public static bool GetWritePermissionForTab(string tabName)
        {
            if (CurrentPermissions == null)
            {
                MessageBox.Show("Права пользователя отсутствуют.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            var tabPermission = CurrentPermissions.Tabs.FirstOrDefault(tab => tab.Name == tabName);

            if (tabPermission != null)
            {
                return tabPermission.Permissions.Write;
            }

            MessageBox.Show($"Вкладка с именем \"{tabName}\" не найдена.", "Информация",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return false;
        }

        // Проверка прав на запись для справочника
        public static bool GetWritePermissionForDict(string dictName)
        {
            if (CurrentPermissions == null)
            {
                MessageBox.Show("Права пользователя отсутствуют.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            var dictPermission = CurrentPermissions.Directories.FirstOrDefault(dict => dict.Name == dictName);

            if (dictPermission != null)
            {
                return dictPermission.Permissions.Write;
            }

            MessageBox.Show($"Справочник с именем \"{dictName}\" не найден.", "Информация",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return false;
        }
    }
}