using System.Globalization;
using System.Windows;
using System.Windows.Data;
using course_project_shop_musical_instruments.Models;

namespace course_project_shop_musical_instruments.Helpers;

public class LoginModeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isLoginMode)
        {
            return isLoginMode ? "Вход" : "Регистрация";
        }
        return "Вход";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class ButtonTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isLoginMode)
        {
            return isLoginMode ? "Войти" : "Зарегистрироваться";
        }
        return "Войти";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class ToggleTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isLoginMode)
        {
            return isLoginMode ? "Нет аккаунта? Зарегистрироваться" : "Уже есть аккаунт? Войти";
        }
        return "Нет аккаунта? Зарегистрироваться";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? Visibility.Collapsed : Visibility.Visible;
        }
        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class SortOptionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string sortOption)
        {
            return sortOption switch
            {
                "None" => "Без сортировки",
                "PriceAscending" => "По возрастанию цены",
                "PriceDescending" => "По убыванию цены",
                _ => sortOption
            };
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string displayName)
        {
            return displayName switch
            {
                "Без сортировки" => "None",
                "По возрастанию цены" => "PriceAscending",
                "По убыванию цены" => "PriceDescending",
                _ => value
            };
        }
        return value;
    }
}

public class TimeSpanToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TimeSpan timeSpan)
        {
            return timeSpan.ToString(@"hh\:mm");
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string str && TimeSpan.TryParse(str, out TimeSpan result))
        {
            return result;
        }
        return TimeSpan.Zero;
    }
}

public class StatusConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "Ожидает подтверждения",
                OrderStatus.Confirmed => "Подтвержден",
                OrderStatus.Processing => "В обработке",
                OrderStatus.Shipped => "Отправлен",
                OrderStatus.Delivered => "Доставлен",
                OrderStatus.Cancelled => "Отменен",
                _ => status.ToString()
            };
        }
        return value?.ToString() ?? string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class OrderCancelVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is OrderStatus status)
        {
            // Кнопка отмены видна только для заказов, которые можно отменить
            // Нельзя отменить: Shipped, Delivered, Cancelled
            return status switch
            {
                OrderStatus.Pending => Visibility.Visible,
                OrderStatus.Confirmed => Visibility.Visible,
                OrderStatus.Processing => Visibility.Visible,
                OrderStatus.Shipped => Visibility.Collapsed,
                OrderStatus.Delivered => Visibility.Collapsed,
                OrderStatus.Cancelled => Visibility.Collapsed,
                _ => Visibility.Collapsed
            };
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class OrderDeleteVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is OrderStatus status)
        {
            // Кнопка удаления видна только для заказов, которые можно удалить
            // Нельзя удалить: Shipped, Delivered
            return status switch
            {
                OrderStatus.Pending => Visibility.Visible,
                OrderStatus.Confirmed => Visibility.Visible,
                OrderStatus.Processing => Visibility.Visible,
                OrderStatus.Shipped => Visibility.Collapsed,
                OrderStatus.Delivered => Visibility.Collapsed,
                OrderStatus.Cancelled => Visibility.Visible,
                _ => Visibility.Visible
            };
        }
        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class EditModeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isEditMode)
        {
            return isEditMode ? "Редактировать товар" : "Добавить товар";
        }
        return "Добавить товар";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

