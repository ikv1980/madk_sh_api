using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Project.Interfaces;
using Project.Models;
using Project.Tools;
using Wpf.Ui.Common;
using Wpf.Ui.Controls;
using MessageBox = System.Windows.MessageBox;

namespace Project.Views.Pages.DirectoryPages.Edit
{
    public partial class EditCar : UiWindow, IRefresh
    {
        public event Action RefreshRequested;
        private readonly bool _isEditMode;
        private readonly bool _isDeleteMode;
        private readonly ulong _itemId;

        // Конструктор для добавления данных
        public EditCar()
        {
            InitializeComponent();
            Init();
            _isEditMode = false;
            _isDeleteMode = false;
            Title = "Добавление данных";
            SaveButton.Content = "Добавить";
            SaveButton.Icon = SymbolRegular.AddCircle24;
        }

        // Конструктор для изменения (удаления) данных
        public EditCar(Car item, string button) : this()
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            
            Init();
            _itemId = item.Id;

            // Установка значений в форму
            EditCarMark.SelectedItem =
                DbUtils.db.CarMarks.FirstOrDefault(m => m.Id == item.MarkId);
            EditCarModel.SelectedItem =
                DbUtils.db.CarModels.FirstOrDefault(m => m.Id == item.ModelId);
            EditCarCountry.SelectedItem =
                DbUtils.db.CarCountries.FirstOrDefault(m => m.Id == item.CountryId);
            EditCarType.SelectedItem =
                DbUtils.db.CarTypes.FirstOrDefault(m => m.Id == item.TypeId);
            EditCarColor.SelectedItem =
                DbUtils.db.CarColors.FirstOrDefault(m => m.Id == item.ColorId);
            EditCarVin.Text = item.Vin;
            EditCarPts.Text = item.Pts;
            EditCarDate.SelectedDate = item.DateAt == default 
                ? (DateTime?)null 
                : item.DateAt.ToDateTime(TimeOnly.MinValue);
            ShowCarBlock.Text = (item.Block != 0  ? "В заказе №" + item.Block.ToString() : "Свободна к продаже");
            ShowCarBlock.Background = item.Block != 0 ? Brushes.Pink : Brushes.LightGreen;
            EditPrice.Text = item.Price.ToString();
            //DisplayImage();

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
            if (button == "Delete")
            {
                _isDeleteMode = true;
                Title = "Удаление данных";
                SaveButton.Content = "Удалить";
                SaveButton.Icon = SymbolRegular.Delete24;
            }
        }

        // Обработка нажатия кнопки "Сохранить"
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var item = (_isEditMode || _isDeleteMode)
                    ? DbUtils.db.Cars.FirstOrDefault(x => x.Id == _itemId)
                    : new Car();

                if (item == null)
                {
                    MessageBox.Show("Данные не найдены.", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Удаление
                if (_isDeleteMode)
                {
                    item.DeletedAt = DateTime.Now; //DbUtils.db.Cars.Remove(item);   
                }
                else
                {
                    if (!IsValidInput())
                        return;

                    // Изменение или добавление
                    UpdateItem(item);

                    if (!_isEditMode)
                    {
                        DbUtils.db.Cars.Add(item);
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
            Close();
        }

        // Инициализация данных для списков
        private void Init()
        {
            EditCarMark.ItemsSource = DbUtils.db.CarMarks.Where(x => x.DeletedAt == null).ToList();
            EditCarModel.ItemsSource = DbUtils.db.CarModels.Where(x => x.DeletedAt == null).ToList();
            EditCarCountry.ItemsSource = DbUtils.db.CarCountries.Where(x => x.DeletedAt == null).ToList();
            EditCarType.ItemsSource = DbUtils.db.CarTypes.Where(x => x.DeletedAt == null).ToList();
            EditCarColor.ItemsSource = DbUtils.db.CarColors.Where(x => x.DeletedAt == null).ToList();
        }

        // Валидация данных
        private bool IsValidInput()
        {
            if (EditCarMark.SelectedItem == null)
            {
                MessageBox.Show("Не выбрана марка", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (EditCarModel.SelectedItem == null)
            {
                MessageBox.Show("Не выбрана модель", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (EditCarCountry.SelectedItem == null)
            {
                MessageBox.Show("Не выбрана страна", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (EditCarType.SelectedItem == null)
            {
                MessageBox.Show("Не выбран тип кузова", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (EditCarColor.SelectedItem == null)
            {
                MessageBox.Show("Не выбран цвет", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(EditCarVin.Text))
            {
                MessageBox.Show("Требуется заполнить VIN код", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(EditCarPts.Text))
            {
                MessageBox.Show("Требуется заполнить ПТС", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(EditCarDate.Text))
            {
                MessageBox.Show("Требуется заполнить дату производства", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(EditPrice.Text))
            {
                MessageBox.Show("Требуется указать цену", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!_isEditMode && !_isDeleteMode)
            {
                var vin = EditCarVin.Text.Trim();
                var pts = EditCarPts.Text.Trim();

                if (DbUtils.db.Cars.Any(x => x.Vin == vin))
                {
                    MessageBox.Show("Такой VIN уже существует", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                if (DbUtils.db.Cars.Any(x => x.Pts == pts))
                {
                    MessageBox.Show("Такой ПТС уже существует", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }

            return true;
        }

        // Обновление данных объекта
        private void UpdateItem(Car item)
        {
            item.MarkId = (EditCarMark.SelectedItem as CarMark)?.Id ?? item.MarkId;
            item.ModelId = (EditCarModel.SelectedItem as CarModel)?.Id ?? item.ModelId;
            item.CountryId = (EditCarCountry.SelectedItem as CarCountry)?.Id ?? item.CountryId;
            item.TypeId = (EditCarType.SelectedItem as CarType)?.Id ?? item.TypeId;
            item.ColorId = (EditCarColor.SelectedItem as CarColor)?.Id ?? item.ColorId;
            item.Vin = EditCarVin.Text.Trim().ToUpper();
            item.Pts = EditCarPts.Text.Trim().ToUpper();
            item.DateAt = EditCarDate.SelectedDate.HasValue
                ? DateOnly.FromDateTime(EditCarDate.SelectedDate.Value)
                : DateOnly.MinValue; // или другое значение по умолчанию
            item.Price = decimal.TryParse(EditPrice.Text.Trim(), out decimal price) ? price : 0;
        }

        // Фокус на элементе
        private void UiWindow_Loaded(object sender, RoutedEventArgs e)
        {
            EditCarMark.Focus();
        }

        // Выбор марки ТС
        private void SelectionMark(object sender, SelectionChangedEventArgs e)
        {
            EditCarModel.IsEnabled = true;

            var selectMark = EditCarMark.SelectedItem as CarMark;

            if (selectMark != null)
            {
                var models = DbUtils.db.CarMarkModelCountries
                    .Where(x => x.MarkId == selectMark.Id)
                    .Select(x => x.Model)
                    .ToList();

                if (models.Any())
                {
                    EditCarModel.ItemsSource = models;
                }
                else
                {
                    EditCarModel.ItemsSource = null;
                    MessageBox.Show("Для выбранной марки нет доступных моделей.", "Информация");
                }
            }
            else
            {
                EditCarModel.ItemsSource = null;
                EditCarModel.IsEnabled = false;
            }
        }

        // Выбор страны производства
        private void SelectionModel(object sender, SelectionChangedEventArgs e)
        {
            EditCarCountry.IsEnabled = true;

            var selectMark = EditCarMark.SelectedItem as CarMark;
            var selectModel = EditCarModel.SelectedItem as CarModel;

            if (selectMark != null && selectModel != null)
            {
                var countries = DbUtils.db.CarMarkModelCountries
                    .Where(x => x.MarkId == selectMark.Id && x.ModelId == selectModel.Id)
                    .Select(x => x.Country)
                    .ToList();

                if (countries.Count > 1)
                {
                    EditCarCountry.ItemsSource = countries;
                    EditCarCountry.SelectedItem = null;
                }
                else if (countries.Count == 1)
                {
                    EditCarCountry.ItemsSource = countries;
                    EditCarCountry.SelectedItem = countries.First();
                }
                else
                {
                    EditCarCountry.ItemsSource = null;
                    MessageBox.Show("Для выбранной марки и модели нет данных о странах производства.", "Информация");
                }
            }
            else
            {
                EditCarCountry.ItemsSource = null;
                EditCarCountry.IsEnabled = false;
            }
        }

        // Добавление фотографии
        private void UploadImage_Click(object sender, RoutedEventArgs e)
        {
            // Открываем диалог выбора файла
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp",
                Title = "Выберите изображение автомобиля"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Загружаем изображение
                    string filePath = openFileDialog.FileName;
                    BitmapImage bitmap = new BitmapImage(new Uri(filePath));
                    EditCarImage.Source = bitmap;
                    
                    MessageBox.Show("Фото успешно загружено и сохранено в базе данных.", "Успех", 
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении фото: {ex.Message}", "Ошибка", 
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        // Сжатие изображения
        private byte[] CompressImage(string filePath, int quality)
        {
            using (var originalImage = System.Drawing.Image.FromFile(filePath))
            {
                var jpegEncoder = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders()
                    .FirstOrDefault(codec => codec.FormatID == System.Drawing.Imaging.ImageFormat.Jpeg.Guid);
                if (jpegEncoder == null)
                    throw new Exception("JPEG encoder not found");

                var encoderParameters = new System.Drawing.Imaging.EncoderParameters(1)
                {
                    Param = { [0] = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality) }
                };

                using (var memoryStream = new MemoryStream())
                {
                    originalImage.Save(memoryStream, jpegEncoder, encoderParameters);
                    return memoryStream.ToArray();
                }
            }
        }
        
        // Отображение фотографии
        /*private void DisplayImage(byte[] imageBytes)
        {
            if (imageBytes != null && imageBytes.Length > 0)
            {
                using (MemoryStream ms = new MemoryStream(imageBytes))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = ms;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    EditCarImage.Source = bitmap;
                }
            }
            else
            {
                MessageBox.Show("Фото отсутствует.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }*/

        // Цена: ввод только цифр
        private void EditPrice_Input(object sender, TextCompositionEventArgs e)
        {
            char inputChar = e.Text[0];
            if (!char.IsDigit(inputChar))
            {
                e.Handled = true; // Отменяем ввод, если не цифра
            }
        }
    }
}