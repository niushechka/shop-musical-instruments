using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using course_project_shop_musical_instruments.Models;
using course_project_shop_musical_instruments.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace course_project_shop_musical_instruments.ViewModels;

public partial class AdminOrdersViewModel : BaseViewModel
{
    private readonly OrderService _orderService;
    private readonly NavigationService _navigationService;

    [ObservableProperty]
    private ObservableCollection<Order> orders = new();

    [ObservableProperty]
    private Order? selectedOrder;

    [ObservableProperty]
    private OrderStatus selectedStatus;

    public List<OrderStatus> Statuses { get; } = Enum.GetValues(typeof(OrderStatus)).Cast<OrderStatus>().ToList();

    public AdminOrdersViewModel(OrderService orderService, NavigationService navigationService)
    {
        _orderService = orderService;
        _navigationService = navigationService;
        
        // Инициализируем коллекции
        Orders = new ObservableCollection<Order>();
        
        // Загружаем заказы асинхронно
        var dispatcher = System.Windows.Application.Current?.Dispatcher ?? System.Windows.Threading.Dispatcher.CurrentDispatcher;
        dispatcher.BeginInvoke(new Action(async () =>
        {
            try
            {
                await LoadOrdersAsync();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Ошибка при загрузке заказов:\n{ex.Message}",
                    "Ошибка",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning);
            }
        }), System.Windows.Threading.DispatcherPriority.Loaded);
    }

    private async Task LoadOrdersAsync()
    {
        try
        {
            // База данных уже инициализирована при запуске приложения
            var allOrders = await _orderService.GetAllOrdersAsync();
            Orders = new ObservableCollection<Order>(allOrders.OrderByDescending(o => o.OrderDate));
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
    
    private async void LoadOrders()
    {
        await LoadOrdersAsync();
    }

    partial void OnSelectedOrderChanged(Order? value)
    {
        if (value != null)
        {
            SelectedStatus = value.Status;
        }
    }

    [RelayCommand]
    private async Task UpdateOrderStatus()
    {
        if (SelectedOrder == null) return;

        try
        {
            await _orderService.UpdateOrderStatusAsync(SelectedOrder.Id, SelectedStatus);
            System.Windows.MessageBox.Show("Статус заказа обновлен", "Успех", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            LoadOrders();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task DeleteOrder(Order order)
    {
        if (order == null) return;

        // Проверяем, нельзя ли удалить заказ со статусом "Отправлен" или "Доставлен"
        if (order.Status == OrderStatus.Shipped || order.Status == OrderStatus.Delivered)
        {
            System.Windows.MessageBox.Show("Нельзя удалить заказ, который уже отправлен или доставлен", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        var result = System.Windows.MessageBox.Show(
            $"Вы уверены, что хотите удалить заказ #{order.Id}?",
            "Подтверждение",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Question);

        if (result == System.Windows.MessageBoxResult.Yes)
        {
            try
            {
                await _orderService.DeleteOrderAsync(order.Id);
                System.Windows.MessageBox.Show("Заказ успешно удален", "Успех", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                LoadOrders();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
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

