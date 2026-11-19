using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace course_project_shop_musical_instruments.Helpers;

public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isInverse = parameter?.ToString() == "Inverse";
        bool isNull = value == null || (value is string str && string.IsNullOrWhiteSpace(str));
        
        if (isInverse)
        {
            return isNull ? Visibility.Visible : Visibility.Collapsed;
        }
        return isNull ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

