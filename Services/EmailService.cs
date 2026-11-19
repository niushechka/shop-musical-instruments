using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using course_project_shop_musical_instruments.Models;
using System;
using System.IO;
using System.Net.Sockets;

namespace course_project_shop_musical_instruments.Services;

public class EmailService
{
    // Настройки SMTP сервера (можно вынести в конфигурационный файл)
    private readonly string _smtpHost = "smtp.gmail.com"; // Для Gmail, измените на свой SMTP сервер
    private readonly int _smtpPort = 587;
    private readonly string _smtpUsername = "niushechka1@gmail.com"; // Замените на ваш email
    private readonly string _smtpPassword = "tjdh lzvj rorg gfha"; // Замените на пароль приложения
    private readonly string _fromEmail = "niushechka1@gmail.com"; // Email отправителя
    private readonly string _fromName = "Магазин музыкальных инструментов PianoBar";
    private readonly string _logFilePath;

    public EmailService()
    {
        // Определяем путь к файлу логов
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        _logFilePath = Path.Combine(baseDirectory, "email_log.txt");
        
        // Логируем путь к файлу при создании сервиса
        System.Diagnostics.Debug.WriteLine($"[EmailService] Инициализация. Путь к логам: {_logFilePath}");
        LogMessage("EmailService инициализирован");
    }

    public async Task<bool> SendOrderConfirmationEmailAsync(Order order, User user)
    {
        string errorDetails = string.Empty;
        try
        {
            System.Diagnostics.Debug.WriteLine($"[EmailService] ===== НАЧАЛО ОТПРАВКИ EMAIL =====");
            System.Diagnostics.Debug.WriteLine($"[EmailService] Заказ ID: {order?.Id}");
            System.Diagnostics.Debug.WriteLine($"[EmailService] Пользователь: {user?.FirstName} {user?.LastName}");
            System.Diagnostics.Debug.WriteLine($"[EmailService] Email получателя: {user?.Email}");
            System.Diagnostics.Debug.WriteLine($"[EmailService] Путь к логам: {_logFilePath}");
            
            LogMessage($"Попытка отправки email для заказа #{order.Id} на адрес {user.Email}");

            // Валидация email адреса получателя
            if (string.IsNullOrWhiteSpace(user.Email) || !user.Email.Contains("@"))
            {
                errorDetails = $"Некорректный email адрес получателя: {user.Email}";
                LogError(errorDetails);
                return false;
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_fromName, _fromEmail));
            message.To.Add(new MailboxAddress($"{user.FirstName} {user.LastName}", user.Email));
            message.Subject = $"Подтверждение заказа #{order.Id}";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = GenerateOrderEmailHtml(order, user)
            };

            message.Body = bodyBuilder.ToMessageBody();

            LogMessage($"Подключение к SMTP серверу {_smtpHost}:{_smtpPort}");

            using var client = new SmtpClient();
            
            // Подключение с таймаутом
            await client.ConnectAsync(_smtpHost, _smtpPort, SecureSocketOptions.StartTls);
            LogMessage("Подключение к SMTP серверу успешно");

            // Аутентификация
            LogMessage($"Аутентификация пользователя {_smtpUsername}");
            await client.AuthenticateAsync(_smtpUsername, _smtpPassword);
            LogMessage("Аутентификация успешна");

            // Отправка письма
            LogMessage($"Отправка письма на адрес {user.Email}");
            var response = await client.SendAsync(message);
            LogMessage($"Письмо успешно отправлено. Ответ сервера: {response}");

            await client.DisconnectAsync(true);
            LogMessage($"Email для заказа #{order.Id} успешно отправлен на {user.Email}");
            return true;
        }
        catch (SmtpCommandException ex)
        {
            errorDetails = $"SMTP ошибка команды: {ex.Message}\n" +
                          $"Код ошибки: {ex.StatusCode}\n" +
                          $"Ответ сервера: {ex.ErrorCode}\n" +
                          $"Stack trace: {ex.StackTrace}";
            LogError($"Ошибка SMTP команды при отправке email для заказа #{order.Id}:\n{errorDetails}");
            
            // Специфичные ошибки SMTP
            if (ex.StatusCode == SmtpStatusCode.AuthenticationInvalidCredentials || 
                ex.StatusCode == SmtpStatusCode.AuthenticationRequired ||
                ex.StatusCode == SmtpStatusCode.AuthenticationMechanismTooWeak ||
                ex.Message.Contains("authentication", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("535", StringComparison.OrdinalIgnoreCase))
            {
                LogError("ОШИБКА АУТЕНТИФИКАЦИИ: Проверьте правильность email и пароля приложения!");
            }
            else if (ex.StatusCode == SmtpStatusCode.MailboxUnavailable || 
                     ex.StatusCode == SmtpStatusCode.MailboxNameNotAllowed)
            {
                LogError($"ОШИБКА: Почтовый ящик недоступен. Проверьте адрес получателя: {user.Email}");
            }
            
            return false;
        }
        catch (SmtpProtocolException ex)
        {
            errorDetails = $"SMTP протокольная ошибка: {ex.Message}\n" +
                          $"Stack trace: {ex.StackTrace}";
            LogError($"Протокольная ошибка SMTP при отправке email для заказа #{order.Id}:\n{errorDetails}");
            return false;
        }
        catch (AuthenticationException ex)
        {
            errorDetails = $"Ошибка аутентификации: {ex.Message}\n" +
                          $"Stack trace: {ex.StackTrace}";
            LogError($"ОШИБКА АУТЕНТИФИКАЦИИ при отправке email для заказа #{order.Id}:\n{errorDetails}");
            LogError("ПРОВЕРЬТЕ: Правильность пароля приложения Gmail (не обычного пароля!)");
            return false;
        }
        catch (SocketException ex)
        {
            errorDetails = $"Ошибка подключения к SMTP серверу: {ex.Message}\n" +
                          $"Проверьте интернет-соединение и доступность сервера {_smtpHost}:{_smtpPort}\n" +
                          $"Stack trace: {ex.StackTrace}";
            LogError($"Ошибка подключения при отправке email для заказа #{order.Id}:\n{errorDetails}");
            return false;
        }
        catch (Exception ex)
        {
            errorDetails = $"Общая ошибка при отправке email: {ex.Message}\n" +
                          $"Тип ошибки: {ex.GetType().Name}\n" +
                          $"Stack trace: {ex.StackTrace}";
            LogError($"Неожиданная ошибка при отправке email для заказа #{order.Id}:\n{errorDetails}");
            return false;
        }
    }

    private void LogMessage(string message)
    {
        var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] INFO: {message}";
        System.Diagnostics.Debug.WriteLine(logEntry);
        WriteToLogFile(logEntry);
    }

    private void LogError(string error)
    {
        var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR: {error}";
        System.Diagnostics.Debug.WriteLine(logEntry);
        WriteToLogFile(logEntry);
    }

    private void WriteToLogFile(string logEntry)
    {
        try
        {
            // Убеждаемся, что директория существует
            var directory = Path.GetDirectoryName(_logFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
            System.Diagnostics.Debug.WriteLine($"[EmailService] Запись в лог файл: {_logFilePath}");
        }
        catch (Exception ex)
        {
            // Логируем ошибку записи в файл, но не прерываем работу приложения
            System.Diagnostics.Debug.WriteLine($"[EmailService] ОШИБКА записи в файл логов: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[EmailService] Путь к файлу: {_logFilePath}");
        }
    }

    private string GenerateOrderEmailHtml(Order order, User user)
    {
        var itemsHtml = string.Empty;
        var itemNumber = 1;

        foreach (var item in order.OrderItems)
        {
            var productName = item.Product?.Name ?? "Товар";
            itemsHtml += $@"
                <tr>
                    <td style=""padding: 10px; border-bottom: 1px solid #ddd;"">{itemNumber}</td>
                    <td style=""padding: 10px; border-bottom: 1px solid #ddd;"">{productName}</td>
                    <td style=""padding: 10px; border-bottom: 1px solid #ddd; text-align: center;"">{item.Quantity}</td>
                    <td style=""padding: 10px; border-bottom: 1px solid #ddd; text-align: right;"">{item.UnitPrice:N2} BYN</td>
                    <td style=""padding: 10px; border-bottom: 1px solid #ddd; text-align: right;"">{item.Subtotal:N2} BYN</td>
                </tr>";
            itemNumber++;
        }

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: #f9f9f9; padding: 20px; border-radius: 0 0 5px 5px; }}
        .order-info {{ background-color: white; padding: 15px; margin: 15px 0; border-radius: 5px; }}
        table {{ width: 100%; border-collapse: collapse; margin: 15px 0; background-color: white; }}
        th {{ background-color: #4CAF50; color: white; padding: 12px; text-align: left; }}
        .total {{ font-size: 18px; font-weight: bold; color: #4CAF50; text-align: right; padding: 15px; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Заказ успешно оформлен!</h1>
        </div>
        <div class=""content"">
            <p>Здравствуйте, {user.FirstName} {user.LastName}!</p>
            <p>Ваш заказ успешно принят в обработку. Ниже представлена информация о вашем заказе:</p>
            
            <div class=""order-info"">
                <p><strong>Номер заказа:</strong> #{order.Id}</p>
                <p><strong>Дата заказа:</strong> {order.OrderDate:dd.MM.yyyy HH:mm}</p>
                <p><strong>Дата доставки:</strong> {order.DeliveryDate:dd.MM.yyyy}</p>
                <p><strong>Время доставки:</strong> {order.DeliveryTime.ToString(@"hh\:mm")}</p>
                <p><strong>Адрес доставки:</strong> {order.DeliveryAddress}</p>
                {(string.IsNullOrWhiteSpace(order.Notes) ? "" : $"<p><strong>Примечания:</strong> {order.Notes}</p>")}
                <p><strong>Статус:</strong> {GetOrderStatusText(order.Status)}</p>
            </div>

            <h3>Состав заказа:</h3>
            <table>
                <thead>
                    <tr>
                        <th style=""width: 5%;"">№</th>
                        <th style=""width: 40%;"">Товар</th>
                        <th style=""width: 15%; text-align: center;"">Количество</th>
                        <th style=""width: 20%; text-align: right;"">Цена за единицу</th>
                        <th style=""width: 20%; text-align: right;"">Сумма</th>
                    </tr>
                </thead>
                <tbody>
                    {itemsHtml}
                </tbody>
            </table>

            <div class=""total"">
                <p>Итого к оплате: {order.TotalAmount:N2} BYN</p>
            </div>

            <p>Спасибо за ваш заказ!</p>
        </div>
        <div class=""footer"">
            <p>Это автоматическое письмо. Пожалуйста, не отвечайте на него.</p>
            <p>&copy; 2024 Магазин музыкальных инструментов</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GetOrderStatusText(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Pending => "Ожидает подтверждения",
            OrderStatus.Confirmed => "Подтвержден",
            OrderStatus.Processing => "В обработке",
            OrderStatus.Shipped => "Отправлен",
            OrderStatus.Delivered => "Доставлен",
            OrderStatus.Cancelled => "Отменен",
            _ => "Неизвестно"
        };
    }

    /// <summary>
    /// Тестирует подключение к SMTP серверу и аутентификацию
    /// Используйте этот метод для диагностики проблем с отправкой email
    /// </summary>
    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            LogMessage("=== Начало тестирования SMTP подключения ===");
            LogMessage($"Попытка подключения к {_smtpHost}:{_smtpPort}");

            using var client = new SmtpClient();
            await client.ConnectAsync(_smtpHost, _smtpPort, SecureSocketOptions.StartTls);
            LogMessage("✓ Подключение к SMTP серверу успешно");

            LogMessage($"Попытка аутентификации пользователя {_smtpUsername}");
            await client.AuthenticateAsync(_smtpUsername, _smtpPassword);
            LogMessage("✓ Аутентификация успешна");

            await client.DisconnectAsync(true);
            LogMessage("✓ Отключение от SMTP сервера успешно");
            LogMessage("=== Тестирование завершено успешно ===");
            return true;
        }
        catch (SmtpCommandException ex)
        {
            LogError($"✗ Ошибка SMTP команды: {ex.Message}");
            LogError($"  Код ошибки: {ex.StatusCode}");
            if (ex.StatusCode == SmtpStatusCode.AuthenticationInvalidCredentials || 
                ex.StatusCode == SmtpStatusCode.AuthenticationRequired ||
                ex.StatusCode == SmtpStatusCode.AuthenticationMechanismTooWeak ||
                ex.Message.Contains("authentication", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("535", StringComparison.OrdinalIgnoreCase))
            {
                LogError("  ПРОВЕРЬТЕ: Правильность пароля приложения Gmail!");
            }
            return false;
        }
        catch (SmtpProtocolException ex)
        {
            LogError($"✗ Протокольная ошибка SMTP: {ex.Message}");
            return false;
        }
        catch (AuthenticationException ex)
        {
            LogError($"✗ Ошибка аутентификации: {ex.Message}");
            LogError("  ПРОВЕРЬТЕ: Правильность email и пароля приложения!");
            return false;
        }
        catch (SocketException ex)
        {
            LogError($"✗ Ошибка подключения: {ex.Message}");
            LogError($"  Проверьте интернет-соединение и доступность сервера {_smtpHost}:{_smtpPort}");
            return false;
        }
        catch (Exception ex)
        {
            LogError($"✗ Неожиданная ошибка: {ex.Message}");
            LogError($"  Тип ошибки: {ex.GetType().Name}");
            return false;
        }
    }
}


