using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using course_project_shop_musical_instruments.Models;
using course_project_shop_musical_instruments.Services;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace course_project_shop_musical_instruments.ViewModels;

public partial class CatalogViewModel : BaseViewModel
{
    private readonly ProductService _productService;
    private readonly CartService _cartService;
    private readonly AuthService _authService;
    private readonly NavigationService _navigationService;

    [ObservableProperty]
    private ObservableCollection<Product> products = new();

    [ObservableProperty]
    private ObservableCollection<string> categories = new();

    [ObservableProperty]
    private ObservableCollection<string> manufacturers = new();

    [ObservableProperty]
    private string selectedCategory = string.Empty;

    [ObservableProperty]
    private string selectedManufacturer = string.Empty;

    [ObservableProperty]
    private string searchTerm = string.Empty;

    [ObservableProperty]
    private string minPrice = string.Empty;

    [ObservableProperty]
    private string maxPrice = string.Empty;

    [ObservableProperty]
    private string selectedSortOption = "None";

    [ObservableProperty]
    private Product? selectedProduct;

    [ObservableProperty]
    private bool isAuthenticated;

    public string[] SortOptions { get; } = { "None", "PriceAscending", "PriceDescending" };

    public CatalogViewModel(ProductService productService, CartService cartService, AuthService authService, NavigationService navigationService)
    {
        _productService = productService;
        _cartService = cartService;
        _authService = authService;
        _navigationService = navigationService;
        
        // Инициализируем пустые коллекции
        Products = new ObservableCollection<Product>();
        Categories = new ObservableCollection<string>();
        Manufacturers = new ObservableCollection<string>();
        
        // Проверяем статус авторизации
        UpdateAuthenticationStatus();
        
        // НЕ загружаем продукты здесь - это будет сделано в Loaded событии окна
        // для гарантированной загрузки после инициализации UI
    }

    public void UpdateAuthenticationStatus()
    {
        IsAuthenticated = _authService.CurrentUser != null;
    }

    partial void OnSearchTermChanged(string value)
    {
        // Автоматически применяем фильтры при изменении поискового запроса
        _ = ApplyFiltersAsync();
    }

    partial void OnSelectedSortOptionChanged(string value)
    {
        // Автоматически применяем фильтры при изменении сортировки
        _ = ApplyFiltersAsync();
    }

    public async Task LoadProductsAsync()
    {
        try
        {
            Debug.WriteLine("CatalogViewModel: LoadProductsAsync started");
            
            // База данных уже инициализирована при запуске приложения
            var services = Services.ServiceLocator.Instance;
            Debug.WriteLine("CatalogViewModel: Getting ServiceLocator");
            
            Debug.WriteLine("CatalogViewModel: Loading products from service");
            var allProducts = await _productService.GetAllProductsAsync();
            Debug.WriteLine($"CatalogViewModel: Loaded {allProducts.Count()} products");
            
            // Обновляем коллекции на UI потоке
            var availableProducts = allProducts.Where(p => p.IsAvailable).ToList();
            Debug.WriteLine($"CatalogViewModel: {availableProducts.Count} available products");
            
            // Применяем сортировку
            availableProducts = SelectedSortOption switch
            {
                "PriceAscending" => availableProducts.OrderBy(p => p.Price).ToList(),
                "PriceDescending" => availableProducts.OrderByDescending(p => p.Price).ToList(),
                _ => availableProducts
            };
            
            Products = new ObservableCollection<Product>(availableProducts);
            Debug.WriteLine($"CatalogViewModel: Products collection updated, count: {Products.Count}");
            
            Categories.Clear();
            Manufacturers.Clear();
            
            var uniqueCategories = allProducts.Select(p => p.CategoryName).Where(c => !string.IsNullOrEmpty(c)).Distinct().OrderBy(c => c).ToList();
            var uniqueManufacturers = allProducts.Select(p => p.ManufacturerName).Where(m => !string.IsNullOrEmpty(m)).Distinct().OrderBy(m => m).ToList();
            
            Debug.WriteLine($"CatalogViewModel: Found {uniqueCategories.Count} categories, {uniqueManufacturers.Count} manufacturers");
            
            foreach (var category in uniqueCategories)
            {
                Categories.Add(category);
            }
            
            foreach (var manufacturer in uniqueManufacturers)
            {
                Manufacturers.Add(manufacturer);
            }
            
            Debug.WriteLine("CatalogViewModel: LoadProductsAsync completed successfully");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"CatalogViewModel LoadProductsAsync ERROR: {ex.Message}\n{ex.StackTrace}");
            MessageBox.Show(
                $"Ошибка при загрузке товаров:\n{ex.Message}\n\nДетали:\n{ex.StackTrace}",
                "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            Products = new ObservableCollection<Product>();
        }
    }
    
    private async void LoadProducts()
    {
        await LoadProductsAsync();
    }

    [RelayCommand]
    private async Task Search()
    {
        await ApplyFiltersAsync();
    }

    [RelayCommand]
    private async Task ApplyFilters()
    {
        await ApplyFiltersAsync();
    }

    private async Task ApplyFiltersAsync()
    {
        try
        {
            // Получаем все продукты
            var allProducts = await _productService.GetAllProductsAsync();
            
            // Применяем поиск по названию, если указан
            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                var term = SearchTerm.ToLower();
                allProducts = allProducts.Where(p =>
                    (p.Name != null && p.Name.ToLower().Contains(term)) ||
                    (p.Description != null && p.Description.ToLower().Contains(term)) ||
                    (p.ManufacturerName != null && p.ManufacturerName.ToLower().Contains(term)) ||
                    (p.CategoryName != null && p.CategoryName.ToLower().Contains(term))
                ).ToList();
            }
            
            // Применяем фильтры
            if (!string.IsNullOrWhiteSpace(SelectedCategory))
            {
                allProducts = allProducts.Where(p => p.CategoryName == SelectedCategory).ToList();
            }
            
            if (!string.IsNullOrWhiteSpace(SelectedManufacturer))
            {
                allProducts = allProducts.Where(p => p.ManufacturerName == SelectedManufacturer).ToList();
            }
            
            // Применяем фильтр по минимальной цене
            if (!string.IsNullOrWhiteSpace(MinPrice) && 
                decimal.TryParse(MinPrice, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal minPriceValue))
            {
                allProducts = allProducts.Where(p => p.Price >= minPriceValue).ToList();
            }
            
            // Применяем фильтр по максимальной цене
            if (!string.IsNullOrWhiteSpace(MaxPrice) && 
                decimal.TryParse(MaxPrice, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal maxPriceValue))
            {
                allProducts = allProducts.Where(p => p.Price <= maxPriceValue).ToList();
            }
            
            // Фильтруем только доступные товары
            var filtered = allProducts.Where(p => p.IsAvailable).ToList();
            
            // Применяем сортировку
            filtered = SelectedSortOption switch
            {
                "PriceAscending" => filtered.OrderBy(p => p.Price).ToList(),
                "PriceDescending" => filtered.OrderByDescending(p => p.Price).ToList(),
                _ => filtered
            };
            
            Products = new ObservableCollection<Product>(filtered);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"CatalogViewModel ApplyFiltersAsync ERROR: {ex.Message}\n{ex.StackTrace}");
            MessageBox.Show(
                $"Ошибка при применении фильтров:\n{ex.Message}",
                "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    [RelayCommand]
    private void ClearFilters()
    {
        SelectedCategory = string.Empty;
        SelectedManufacturer = string.Empty;
        MinPrice = string.Empty;
        MaxPrice = string.Empty;
        SearchTerm = string.Empty;
        SelectedSortOption = "None";
        LoadProducts();
    }

    [RelayCommand]
    private void AddToCart(Product product)
    {
        // Проверяем авторизацию
        if (!IsAuthenticated)
        {
            var result = System.Windows.MessageBox.Show(
                "Для добавления товара в корзину необходимо войти в систему или зарегистрироваться.\n\nХотите войти?",
                "Требуется авторизация",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Question);
            
            if (result == System.Windows.MessageBoxResult.Yes)
            {
                NavigateToLogin();
            }
            return;
        }

        if (product != null && product.StockQuantity > 0)
        {
            _cartService.AddToCart(product, 1);
            System.Windows.MessageBox.Show($"{product.Name} добавлен в корзину", "Успех", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }
        else
        {
            System.Windows.MessageBox.Show("Товар недоступен", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
        }
    }

    [RelayCommand]
    private void ViewProductDetails(Product product)
    {
        if (product == null) return;
        
        var productDetailsWindow = new Views.ProductDetailsView(product);
        productDetailsWindow.ShowDialog();
    }

    [RelayCommand]
    private void NavigateToCart()
    {
        _navigationService.NavigateToCart();
    }

    [RelayCommand]
    private void NavigateToAccount()
    {
        _navigationService.NavigateToAccount();
    }

    [RelayCommand]
    private void Logout()
    {
        try
        {
            Debug.WriteLine("CatalogViewModel: Logout started");
            _authService.Logout();
            Debug.WriteLine("CatalogViewModel: User logged out");
            UpdateAuthenticationStatus();
            Debug.WriteLine("CatalogViewModel: Authentication status updated");
        }
        catch (Exception ex)
        {
            var errorMsg = $"Ошибка при выходе:\n{ex.Message}";
            Debug.WriteLine($"CatalogViewModel Logout ERROR: {errorMsg}");
            MessageBox.Show(errorMsg, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void NavigateToLogin()
    {
        _navigationService.NavigateToLogin();
    }

    [RelayCommand]
    private void NavigateToRegister()
    {
        _navigationService.NavigateToLogin(registerMode: true);
    }
}

