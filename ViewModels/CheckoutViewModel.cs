using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using course_project_shop_musical_instruments.Models;
using course_project_shop_musical_instruments.Services;
using System.Collections.ObjectModel;

namespace course_project_shop_musical_instruments.ViewModels;

public partial class CheckoutViewModel : BaseViewModel
{
    private readonly CartService _cartService;
    private readonly OrderService _orderService;
    private readonly AuthService _authService;
    private readonly NavigationService _navigationService;

    [ObservableProperty]
    private ObservableCollection<CartItem> cartItems = new();

    [ObservableProperty]
    private decimal total;

    [ObservableProperty]
    private DateTime deliveryDate = DateTime.Now.AddDays(1);

    public DateTime MaxDeliveryDate => DateTime.Today.AddMonths(1);

    [ObservableProperty]
    private TimeSpan deliveryTime = new TimeSpan(10, 0, 0);

    [ObservableProperty]
    private string deliveryAddress = string.Empty;

    public string DeliveryTimeDisplay => DeliveryTime.ToString(@"hh\:mm");

    [ObservableProperty]
    private string? notes;

    public ObservableCollection<TimeSpan> DeliveryTimeOptions { get; } = new()
    {
        new TimeSpan(9, 0, 0),
        new TimeSpan(10, 0, 0),
        new TimeSpan(11, 0, 0),
        new TimeSpan(12, 0, 0),
        new TimeSpan(13, 0, 0),
        new TimeSpan(14, 0, 0),
        new TimeSpan(15, 0, 0),
        new TimeSpan(16, 0, 0),
        new TimeSpan(17, 0, 0),
        new TimeSpan(18, 0, 0)
    };

    public CheckoutViewModel(CartService cartService, OrderService orderService, AuthService authService, NavigationService navigationService)
    {
        _cartService = cartService;
        _orderService = orderService;
        _authService = authService;
        _navigationService = navigationService;
        LoadCart();
        
        // Поля остаются пустыми - клиент должен ввести данные самостоятельно
        DeliveryAddress = string.Empty;
        Notes = null;
    }

    private void LoadCart()
    {
        CartItems = new ObservableCollection<CartItem>(_cartService.GetCartItems());
        Total = _cartService.GetTotal();
    }

    partial void OnDeliveryTimeChanged(TimeSpan value)
    {
        OnPropertyChanged(nameof(DeliveryTimeDisplay));
    }

    partial void OnDeliveryAddressChanged(string value)
    {
        // Принудительно обновляем свойство при изменении
        // Это гарантирует, что изменения в TextBox будут сохранены
    }

    [RelayCommand]
    private async Task PlaceOrder()
    {
        // Валидация адреса доставки - принудительно получаем текущее значение
        // и проверяем после Trim
        var currentAddress = DeliveryAddress ?? string.Empty;
        var trimmedAddress = currentAddress.Trim();
        
        // Строгая проверка - адрес не должен быть пустым
        if (string.IsNullOrWhiteSpace(trimmedAddress) || trimmedAddress.Length == 0)
        {
            System.Windows.MessageBox.Show("Пожалуйста, укажите адрес доставки", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            // Фокусируемся на поле адреса
            return;
        }
        
        // Дополнительная проверка - если адрес слишком короткий (меньше 3 символов)
        if (trimmedAddress.Length < 3)
        {
            System.Windows.MessageBox.Show("Адрес доставки должен содержать минимум 3 символа", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        // Валидация даты доставки
        if (DeliveryDate < DateTime.Now.Date)
        {
            System.Windows.MessageBox.Show("Дата доставки не может быть в прошлом", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        // Валидация максимальной даты доставки (не более месяца вперед)
        if (DeliveryDate > MaxDeliveryDate)
        {
            System.Windows.MessageBox.Show($"Дата доставки не может быть позже {MaxDeliveryDate:dd.MM.yyyy}", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        // Валидация времени доставки
        if (DeliveryTime == default(TimeSpan))
        {
            System.Windows.MessageBox.Show("Пожалуйста, выберите время доставки", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        // Валидация пользователя
        if (_authService.CurrentUser == null)
        {
            System.Windows.MessageBox.Show("Необходимо войти в систему", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            _navigationService.NavigateToLogin();
            return;
        }

        // Валидация корзины
        if (CartItems == null || CartItems.Count == 0)
        {
            System.Windows.MessageBox.Show("Корзина пуста", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        try
        {
            // Финальная проверка перед отправкой - убеждаемся, что адрес действительно не пустой
            if (string.IsNullOrWhiteSpace(trimmedAddress) || trimmedAddress.Trim().Length == 0)
            {
                System.Windows.MessageBox.Show("Адрес доставки не может быть пустым. Пожалуйста, укажите полный адрес доставки.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }
            
            // Убеждаемся, что используем очищенный адрес
            var finalAddress = trimmedAddress.Trim();
            
            var orderId = await _orderService.CreateOrderAsync(
                _authService.CurrentUser.Id,
                CartItems.ToList(),
                DeliveryDate,
                DeliveryTime,
                finalAddress, // Используем финальный очищенный адрес
                Notes
            );

            _cartService.ClearCart();
            
            // Небольшая задержка, чтобы убедиться, что транзакция полностью завершена
            await Task.Delay(100);
            
            System.Windows.MessageBox.Show($"Заказ #{orderId} успешно оформлен!", "Успех", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            _navigationService.NavigateToAccount();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Ошибка при оформлении заказа: {ex.Message}", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void NavigateToCart()
    {
        _navigationService.NavigateToCart();
    }
}

