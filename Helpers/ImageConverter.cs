using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace course_project_shop_musical_instruments.Helpers;

public class ImageConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        try
        {
            // Проверяем ImageData (BLOB) первым
            if (values != null && values.Length > 0 && values[0] is byte[] imageData && imageData.Length > 0)
            {
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = new MemoryStream(imageData);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    return bitmap;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"ImageConverter: Error loading BLOB image: {ex.Message}");
                }
            }
            
            // Если ImageData нет, проверяем ImagePath (для обратной совместимости)
            if (values != null && values.Length > 1 && values[1] is string imagePath && !string.IsNullOrWhiteSpace(imagePath))
            {
                if (!Path.IsPathRooted(imagePath))
                {
                    string basePath = AppDomain.CurrentDomain.BaseDirectory;
                    imagePath = Path.Combine(basePath, "Images", imagePath);
                }

                if (File.Exists(imagePath))
                {
                    try
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        bitmap.Freeze();
                        return bitmap;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"ImageConverter: Error loading file image: {ex.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ImageConverter ERROR: {ex.Message}");
        }
        
        return null!;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

