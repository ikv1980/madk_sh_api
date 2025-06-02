using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Project.Tools
{
    class Helpers
    {
        public string HashPassword(string password) 
        {
            using (var sha256 = SHA256.Create())
            {
                // Генерация соли
                byte[] salt = Encoding.UTF8.GetBytes("ikv1980MadK");
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] saltedPassword = passwordBytes.Concat(salt).ToArray();

                // Хэширование
                byte[] hash = sha256.ComputeHash(saltedPassword);
                return Convert.ToBase64String(hash);
            }
        }
        
        // Вывод объекта в консоль
        public void PrintObject(object obj)
        {
            if (obj == null)
            {
                Console.WriteLine("Объект равен null.");
                return;
            }
        
            Type type = obj.GetType();
            Console.WriteLine($"____________________________________");
            Console.WriteLine($"Содержимое объекта типа {type.Name}:");
        
            foreach (PropertyInfo property in type.GetProperties())
            {
                var value = property.GetValue(obj);
                Console.WriteLine($"{property.Name}: {value}");
            }
        }
    }
}
