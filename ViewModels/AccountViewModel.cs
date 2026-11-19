using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using course_project_shop_musical_instruments.Models;
using course_project_shop_musical_instruments.Services;
using course_project_shop_musical_instruments.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace course_project_shop_musical_instruments.ViewModels;

public partial class AccountViewModel : BaseViewModel
{
    private readonly AuthService _authService;
    private readonly OrderService _orderService;
    private readonly UserRepository _userRepository;
    private readonly NavigationService _navigationService;

    [ObservableProperty]
    private ObservableCollection<Order> orders = new();

    [ObservableProperty]
    private Order? selectedOrder;

    [ObservableProperty]
    private bool isEditMode;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string firstName = string.Empty;

    [ObservableProperty]
    private string lastName = string.Empty;

    [ObservableProperty]
    private string phone = string.Empty;

    [ObservableProperty]
    private string address = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private string confirmPassword = string.Empty;

    public User? CurrentUser => _authService.CurrentUser;

    public AccountViewModel(AuthService authService, OrderService orderService, UserRepository userRepository, NavigationService navigationService)
    {
        _authService = authService;
        _orderService = orderService;
        _userRepository = userRepository;
        _navigationService = navigationService;
        
        LoadUserData();
        LoadOrders();
    }

    private void LoadUserData()
    {
        if (CurrentUser != null)
        {
            Email = CurrentUser.Email;
            FirstName = CurrentUser.FirstName;
            LastName = CurrentUser.LastName;
            Phone = CurrentUser.Phone ?? string.Empty;
            Address = CurrentUser.Address ?? string.Empty;
        }
    }

    private async void LoadOrders()
    {
        try
        {
            if (CurrentUser != null)
            {
                // База данных уже инициализирована при запуске приложения
                var userOrders = await _orderService.GetUserOrdersAsync(CurrentUser.Id);
                var sortedOrders = userOrders.OrderByDescending(o => o.OrderDate).ToList();
                
                // Обновляем коллекцию, сохраняя выбранный заказ если он есть
                var selectedOrderId = SelectedOrder?.Id;
                Orders = new ObservableCollection<Order>(sortedOrders);
                
                // Восстанавливаем выбранный заказ после обновления
                if (selectedOrderId.HasValue)
                {
                    SelectedOrder = Orders.FirstOrDefault(o => o.Id == selectedOrderId.Value);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Ошибка при загрузке заказов:\n{ex.Message}",
                "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            Orders = new ObservableCollection<Order>();
        }
    }

    [RelayCommand]
    private void EditProfile()
    {
        IsEditMode = true;
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditMode = false;
        LoadUserData();
        Password = string.Empty;
        ConfirmPassword = string.Empty;
    }

    [RelayCommand]
    private async Task SaveProfile()
    {
        if (CurrentUser == null)
        {
            MessageBox.Show("Пользователь не авторизован", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        // Валидация обязательных полей
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName))
        {
            MessageBox.Show("Пожалуйста, заполните все обязательные поля (Email, Имя, Фамилия)", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Проверка email на уникальность (если изменился)
        if (Email != CurrentUser.Email)
        {
            var existingUser = await _userRepository.GetByEmailAsync(Email);
            if (existingUser != null)
            {
                MessageBox.Show("Пользователь с таким email уже существует", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        // Проверка пароля, если он указан
        if (!string.IsNullOrWhiteSpace(Password))
        {
            if (Password != ConfirmPassword)
            {
                MessageBox.Show("Пароли не совпадают", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (Password.Length < 3)
            {
                MessageBox.Show("Пароль должен содержать минимум 3 символа", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        try
        {
            // Обновляем данные пользователя
            CurrentUser.Email = Email;
            CurrentUser.FirstName = FirstName;
            CurrentUser.LastName = LastName;
            CurrentUser.Phone = Phone;
            CurrentUser.Address = Address;

            // Обновляем пароль, если он указан
            if (!string.IsNullOrWhiteSpace(Password))
            {
                CurrentUser.PasswordHash = Password; // В продакшене нужно хешировать пароль
            }

            var success = await _userRepository.UpdateAsync(CurrentUser);
            
            if (success)
            {
                // Обновляем текущего пользователя в AuthService
                await _authService.UpdateCurrentUserAsync();
                
                MessageBox.Show("Данные успешно обновлены", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                IsEditMode = false;
                Password = string.Empty;
                ConfirmPassword = string.Empty;
                
                // Обновляем данные на экране
                LoadUserData();
            }
            else
            {
                MessageBox.Show("Не удалось обновить данные", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при обновлении данных:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void NavigateToCatalog()
    {
        _navigationService.NavigateToCatalog();
    }

    [RelayCommand]
    private void NavigateToCart()
    {
        _navigationService.NavigateToCart();
    }

    [RelayCommand]
    private void Logout()
    {
        try
        {
            _authService.Logout();
            _navigationService.NavigateToLogin();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при выходе:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    partial void OnSelectedOrderChanged(Order? value)
    {
        if (value != null && value.OrderItems == null)
        {
            value.OrderItems = new List<OrderItem>();
        }
        
        // Если OrderItems пуст, пытаемся перезагрузить заказ
        if (value != null && value.OrderItems != null && value.OrderItems.Count == 0)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    var fullOrder = await _orderService.GetOrderByIdAsync(value.Id);
                    if (fullOrder != null && fullOrder.OrderItems != null && fullOrder.OrderItems.Count > 0)
                    {
                        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                        {
                            if (SelectedOrder?.Id == fullOrder.Id)
                            {
                                SelectedOrder = fullOrder;
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"AccountViewModel: Error reloading order: {ex.Message}");
                }
            });
        }
    }

    [RelayCommand]
    private void ViewOrderDetails(Order order)
    {
        SelectedOrder = order;
    }

    public async Task RefreshOrdersAsync()
    {
        await Task.Run(() =>
        {
            System.Windows.Application.Current?.Dispatcher.Invoke(() =>
            {
                LoadOrders();
            });
        });
    }

    [RelayCommand]
    private async Task CancelOrder(Order order)
    {
        if (order == null) return;

        if (order.Status == OrderStatus.Cancelled)
        {
            MessageBox.Show("Заказ уже отменен", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (order.Status == OrderStatus.Shipped || order.Status == OrderStatus.Delivered)
        {
            MessageBox.Show("Нельзя отменить заказ, который уже отправлен или доставлен", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show(
            $"Вы уверены, что хотите отменить заказ #{order.Id}?",
            "Подтверждение отмены",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                var success = await _orderService.UpdateOrderStatusAsync(order.Id, OrderStatus.Cancelled);
                if (success)
                {
                    MessageBox.Show("Заказ успешно отменен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadOrders();
                }
                else
                {
                    MessageBox.Show("Не удалось отменить заказ", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отмене заказа:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    [RelayCommand]
    private async Task DeleteOrder(Order order)
    {
        if (order == null) return;

        // Проверяем, нельзя ли удалить заказ со статусом "Отправлен" или "Доставлен"
        if (order.Status == OrderStatus.Shipped || order.Status == OrderStatus.Delivered)
        {
            MessageBox.Show("Нельзя удалить заказ, который уже отправлен или доставлен", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show(
            $"Вы уверены, что хотите удалить заказ #{order.Id}?",
            "Подтверждение удаления",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                var success = await _orderService.DeleteOrderAsync(order.Id);
                if (success)
                {
                    MessageBox.Show("Заказ успешно удален", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadOrders();
                    
                    if (SelectedOrder?.Id == order.Id)
                    {
                        SelectedOrder = null;
                    }
                }
                else
                {
                    MessageBox.Show("Не удалось удалить заказ", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении заказа:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public string GetStatusText(OrderStatus status)
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
}

