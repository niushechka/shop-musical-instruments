using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using course_project_shop_musical_instruments.Data;
using course_project_shop_musical_instruments.Models;
using course_project_shop_musical_instruments.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace course_project_shop_musical_instruments.ViewModels;

public partial class AdminCustomersViewModel : BaseViewModel
{
    private readonly UserRepository _userRepository;
    private readonly OrderService _orderService;
    private readonly NavigationService _navigationService;

    [ObservableProperty]
    private ObservableCollection<User> customers = new();

    [ObservableProperty]
    private User? selectedCustomer;

    [ObservableProperty]
    private ObservableCollection<Order> customerOrders = new();

    public AdminCustomersViewModel(UserRepository userRepository, OrderService orderService, NavigationService navigationService)
    {
        _userRepository = userRepository;
        _orderService = orderService;
        _navigationService = navigationService;
        
        // Инициализируем коллекции
        Customers = new ObservableCollection<User>();
        CustomerOrders = new ObservableCollection<Order>();
        
        // Загружаем клиентов асинхронно
        var dispatcher = System.Windows.Application.Current?.Dispatcher ?? System.Windows.Threading.Dispatcher.CurrentDispatcher;
        dispatcher.BeginInvoke(new Action(async () =>
        {
            try
            {
                await LoadCustomersAsync();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Ошибка при загрузке клиентов:\n{ex.Message}",
                    "Ошибка",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning);
            }
        }), System.Windows.Threading.DispatcherPriority.Loaded);
    }

    private async Task LoadCustomersAsync()
    {
        try
        {
            // База данных уже инициализирована при запуске приложения
            var allUsers = await _userRepository.GetAllAsync();
            Customers = new ObservableCollection<User>(allUsers.Where(u => u.Role == UserRole.Client));
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Ошибка при загрузке клиентов:\n{ex.Message}",
                "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            Customers = new ObservableCollection<User>();
        }
    }
    
    private async void LoadCustomers()
    {
        await LoadCustomersAsync();
    }

    partial void OnSelectedCustomerChanged(User? value)
    {
        if (value != null)
        {
            LoadCustomerOrders(value.Id);
        }
        else
        {
            CustomerOrders.Clear();
        }
    }

    private async void LoadCustomerOrders(int userId)
    {
        var orders = await _orderService.GetUserOrdersAsync(userId);
        CustomerOrders = new ObservableCollection<Order>(orders.OrderByDescending(o => o.OrderDate));
    }

    [RelayCommand]
    private async Task DeleteCustomer(User user)
    {
        if (user == null) return;

        // Проверяем, не является ли пользователь администратором
        if (user.Role == UserRole.Admin)
        {
            MessageBox.Show("Нельзя удалить администратора", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Проверяем наличие заказов у клиента
        var orders = await _orderService.GetUserOrdersAsync(user.Id);
        if (orders != null && orders.Count > 0)
        {
            var result = MessageBox.Show(
                $"У клиента {user.Email} есть {orders.Count} заказ(ов). Удаление клиента не удалит его заказы.\n\nВы уверены, что хотите удалить этого клиента?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }
        }
        else
        {
            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить клиента '{user.Email}'?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }
        }

        try
        {
            var success = await _userRepository.DeleteAsync(user.Id);
            if (success)
            {
                MessageBox.Show("Клиент успешно удален", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadCustomersAsync();
                
                // Очищаем выбранного клиента, если он был удален
                if (SelectedCustomer?.Id == user.Id)
                {
                    SelectedCustomer = null;
                    CustomerOrders.Clear();
                }
            }
            else
            {
                MessageBox.Show("Не удалось удалить клиента", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при удалении клиента: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

