using System.Net.Mail;
using System.Text.RegularExpressions;

namespace Project.Tools
{
    public class ValidateField
    {
        /// Проверяет значение на соответствие заданному ключу.
        /// <param name="value">Значение для проверки.</param>
        /// <param name="key">Ключ проверки: "password", "email", "phone".</param>
        /// <returns>true, если данные валидны; false, если есть ошибка.</returns>
        public bool IsValid(string value, string key)
        {
            switch (key)
            {
                case "password":
                    return IsPasswordValid(value);
                case "email":
                    return IsEmailValid(value);
                case "phone":
                    return IsPhoneValid(value);
                default:
                    return false; // Неизвестный тип валидации.
            }
        }

        private bool IsPasswordValid(string password)
        {
            return !string.IsNullOrWhiteSpace(password) && password.Length >= 6;
        }

        private bool IsEmailValid(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                new MailAddress(email); // Исключение означает невалидный e-mail.
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool IsPhoneValid(string phone)
        {
            var regex = new Regex(@"^(\+7|8)?\d{10}$");
            return !string.IsNullOrWhiteSpace(phone) && regex.IsMatch(phone);
        }
    }
}