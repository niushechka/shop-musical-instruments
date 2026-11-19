using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using course_project_shop_musical_instruments.Models;
using course_project_shop_musical_instruments.Services;
using System;
using System.Globalization;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows;

namespace course_project_shop_musical_instruments.ViewModels;

public partial class ProductDetailsViewModel : BaseViewModel
{
    private readonly ProductService _productService;
    private readonly CartService _cartService;
    private readonly NavigationService _navigationService;
    private readonly Action? _closeWindowAction;

    [ObservableProperty]
    private Product? product;

    [ObservableProperty]
    private string selectedCurrency = "BYN";

    public string[] Currencies { get; } = { "BYN", "USD", "RUB", "EUR" };

    [ObservableProperty]
    private decimal convertedPrice;

    public ProductDetailsViewModel(Product product, ProductService productService, CartService cartService, NavigationService navigationService, Action? closeWindowAction = null)
    {
        Product = product;
        _productService = productService;
        _cartService = cartService;
        _navigationService = navigationService;
        _closeWindowAction = closeWindowAction;
        UpdateConvertedPrice();
    }

    partial void OnSelectedCurrencyChanged(string value)
    {
        UpdateConvertedPrice();
    }

    private void UpdateConvertedPrice()
    {
        if (Product == null) return;

        // Курсы валют (можно вынести в настройки или получать из API)
        decimal usdRate = 3.2m;
        decimal rubRate = 30m;
        decimal eurRate = 3.5m;

        ConvertedPrice = SelectedCurrency switch
        {
            "USD" => Math.Round(Product.Price / usdRate, 2),
            "RUB" => Math.Round(Product.Price * rubRate, 2),
            "EUR" => Math.Round(Product.Price / eurRate, 2),
            _ => Product.Price
        };
    }

    [RelayCommand]
    private void AddToCart()
    {
        if (Product != null && Product.StockQuantity > 0)
        {
            _cartService.AddToCart(Product, 1);
            MessageBox.Show($"{Product.Name} добавлен в корзину", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            MessageBox.Show("Товар недоступен", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    [RelayCommand]
    private void NavigateToCatalog()
    {
        // Просто закрываем окно карточки товара - каталог уже открыт
        _closeWindowAction?.Invoke();
    }

    public BitmapImage? ProductImage
    {
        get
        {
            if (Product == null) return null;

            try
            {
                // Сначала пытаемся загрузить из BLOB
                if (Product.ImageData != null && Product.ImageData.Length > 0)
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = new MemoryStream(Product.ImageData);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    return bitmap;
                }

                // Если BLOB нет, пытаемся загрузить из файла
                if (!string.IsNullOrEmpty(Product.ImagePath))
                {
                    string imagePath = Product.ImagePath;
                    if (!Path.IsPathRooted(imagePath))
                    {
                        string basePath = AppDomain.CurrentDomain.BaseDirectory;
                        imagePath = Path.Combine(basePath, "Images", imagePath);
                    }

                    if (File.Exists(imagePath))
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        bitmap.Freeze();
                        return bitmap;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading product image: {ex.Message}");
            }

            return null;
        }
    }
}

