using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace course_project_shop_musical_instruments.Helpers;

public class ImagePathConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        try
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                // Возвращаем null для пустого пути
                return null!;
            }

            string imagePath = value.ToString()!;
            
            // Если путь относительный, делаем его абсолютным
            if (!Path.IsPathRooted(imagePath))
            {
                string basePath = AppDomain.CurrentDomain.BaseDirectory;
                imagePath = Path.Combine(basePath, "Images", imagePath);
            }

            // Проверяем существование файла
            if (File.Exists(imagePath))
            {
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze(); // Замораживаем для использования в разных потоках
                    return bitmap;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"ImagePathConverter: Error loading image {imagePath}: {ex.Message}");
                    // Если не удалось загрузить, возвращаем null
                    return null!;
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"ImagePathConverter: Image file not found: {imagePath}");
            }

            // Возвращаем null если файл не найден
            return null!;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ImagePathConverter ERROR: {ex.Message}");
            return null!;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    private BitmapImage GetPlaceholderImage()
    {
        // Возвращаем null, чтобы использовать fallback в XAML
        return null!;
    }
}

