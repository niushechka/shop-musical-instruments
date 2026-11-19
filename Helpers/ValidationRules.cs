using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace course_project_shop_musical_instruments.Helpers;

public class RequiredValidationRule : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
        {
            return new ValidationResult(false, "Это поле обязательно для заполнения");
        }
        return ValidationResult.ValidResult;
    }
}

public class EmailValidationRule : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
        {
            return new ValidationResult(false, "Email обязателен для заполнения");
        }

        string email = value.ToString()!;
        string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        
        if (!Regex.IsMatch(email, pattern))
        {
            return new ValidationResult(false, "Введите корректный email адрес");
        }
        
        return ValidationResult.ValidResult;
    }
}

public class PhoneValidationRule : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        // Проверка обязательности выполняется через RequiredValidationRule
        // Здесь проверяем только формат, если значение не пустое
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
        {
            return ValidationResult.ValidResult; // Обязательность проверяется через RequiredValidationRule
        }

        string phone = value.ToString()!.Trim();
        
        // Проверка длины (максимум 13 символов: + и 12 цифр)
        if (phone.Length > 13)
        {
            return new ValidationResult(false, "Номер телефона не может содержать более 13 символов");
        }
        
        // Проверка формата: должен начинаться с + и содержать только цифры после +
        string pattern = @"^\+375\d{9}$";
        
        if (!Regex.IsMatch(phone, pattern))
        {
            return new ValidationResult(false, "Введите номер в формате +375XXXXXXXXX (например, +375297794420)");
        }
        
        return ValidationResult.ValidResult;
    }
}

public class PriceValidationRule : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
        {
            return new ValidationResult(false, "Цена обязательна для заполнения");
        }

        string stringValue = value.ToString()!;
        if (decimal.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price))
        {
            if (price < 0)
            {
                return new ValidationResult(false, "Цена не может быть отрицательной");
            }
            if (price > 1000000)
            {
                return new ValidationResult(false, "Цена слишком большая");
            }
            return ValidationResult.ValidResult;
        }
        
        return new ValidationResult(false, "Введите корректную цену (только цифры и точка)");
    }
}

public class QuantityValidationRule : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
        {
            return new ValidationResult(false, "Количество обязательно для заполнения");
        }

        if (int.TryParse(value.ToString(), out int quantity))
        {
            if (quantity < 0)
            {
                return new ValidationResult(false, "Количество не может быть отрицательным");
            }
            if (quantity > 100000)
            {
                return new ValidationResult(false, "Количество слишком большое");
            }
            return ValidationResult.ValidResult;
        }
        
        return new ValidationResult(false, "Введите корректное количество");
    }
}

public class OptionalPriceValidationRule : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        // Поле не обязательно, пустое значение допустимо
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
        {
            return ValidationResult.ValidResult;
        }

        string stringValue = value.ToString()!;
        
        // Если значение не пустое, проверяем, что это корректное число
        if (decimal.TryParse(stringValue, NumberStyles.Any, cultureInfo, out decimal price))
        {
            if (price < 0)
            {
                return new ValidationResult(false, "Цена не может быть отрицательной");
            }
            if (price > 1000000)
            {
                return new ValidationResult(false, "Цена слишком большая");
            }
            return ValidationResult.ValidResult;
        }
        
        return new ValidationResult(false, "Введите корректную цену (только цифры и точка)");
    }
}

