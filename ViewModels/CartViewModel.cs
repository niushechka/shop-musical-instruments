using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using course_project_shop_musical_instruments.Models;
using course_project_shop_musical_instruments.Services;
using System.Collections.ObjectModel;

namespace course_project_shop_musical_instruments.ViewModels;

public partial class CartViewModel : BaseViewModel
{
    private readonly CartService _cartService;
    private readonly NavigationService _navigationService;

    [ObservableProperty]
    private ObservableCollection<CartItem> cartItems = new();

    [ObservableProperty]
    private decimal total;

    public CartViewModel(CartService cartService, NavigationService navigationService)
    {
        _cartService = cartService;
        _navigationService = navigationService;
        LoadCart();
    }

    private void LoadCart()
    {
        CartItems = new ObservableCollection<CartItem>(_cartService.GetCartItems());
        UpdateTotal();
    }

    private void UpdateTotal()
    {
        Total = _cartService.GetTotal();
    }

    [RelayCommand]
    private void IncreaseQuantity(CartItem item)
    {
        if (item != null)
        {
            _cartService.UpdateQuantity(item.ProductId, item.Quantity + 1);
            LoadCart();
        }
    }

    [RelayCommand]
    private void DecreaseQuantity(CartItem item)
    {
        if (item != null && item.Quantity > 1)
        {
            _cartService.UpdateQuantity(item.ProductId, item.Quantity - 1);
            LoadCart();
        }
    }

    [RelayCommand]
    private void RemoveItem(CartItem item)
    {
        if (item != null)
        {
            _cartService.RemoveFromCart(item.ProductId);
            CartItems.Remove(item);
            UpdateTotal();
        }
    }

    [RelayCommand]
    private void NavigateToCheckout()
    {
        if (CartItems.Count == 0)
        {
            System.Windows.MessageBox.Show("Корзина пуста", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }
        _navigationService.NavigateToCheckout();
    }

    [RelayCommand]
    private void NavigateToCatalog()
    {
        _navigationService.NavigateToCatalog();
    }
}

