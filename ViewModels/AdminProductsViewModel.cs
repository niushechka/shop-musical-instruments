using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using course_project_shop_musical_instruments.Models;
using course_project_shop_musical_instruments.Services;
using course_project_shop_musical_instruments.Data;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Win32;

namespace course_project_shop_musical_instruments.ViewModels;

public partial class AdminProductsViewModel : BaseViewModel
{
    private readonly ProductService _productService;
    private readonly NavigationService _navigationService;
    private readonly Services.ServiceLocator _services;

    [ObservableProperty]
    private ObservableCollection<Product> products = new();

    [ObservableProperty]
    private Product? selectedProduct;

    [ObservableProperty]
    private bool isEditMode;

    [ObservableProperty]
    private bool isFormVisible;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSave))]
    private string productName = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSave))]
    private string description = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSave))]
    private string price = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSave))]
    private string stockQuantity = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSave))]
    private Category? selectedCategory;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSave))]
    private Manufacturer? selectedManufacturer;

    [ObservableProperty]
    private ObservableCollection<Category> categories = new();

    [ObservableProperty]
    private ObservableCollection<Manufacturer> manufacturers = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSave))]
    private string articleNumber = string.Empty;

    [ObservableProperty]
    private string imagePath = string.Empty;

    [ObservableProperty]
    private byte[]? imageData;

    [ObservableProperty]
    private BitmapImage? previewImage;

    [ObservableProperty]
    private bool isAvailable = true;

    [ObservableProperty]
    private bool isSaveEnabled = false;

    // Проверка возможности сохранения - СТРОГАЯ ВАЛИДАЦИЯ
    public bool CanSave
    {
        get
        {
            var trimmedProductName = ProductName?.Trim() ?? string.Empty;
            var trimmedDescription = Description?.Trim() ?? string.Empty;
            var trimmedPrice = Price?.Trim() ?? string.Empty;
            var trimmedStockQuantity = StockQuantity?.Trim() ?? string.Empty;
            var trimmedArticleNumber = ArticleNumber?.Trim() ?? string.Empty;

            // Используем IsNullOrWhiteSpace для более строгой проверки
            if (string.IsNullOrWhiteSpace(trimmedProductName)) return false;
            if (string.IsNullOrWhiteSpace(trimmedDescription)) return false;
            if (SelectedCategory == null) return false;
            if (SelectedManufacturer == null) return false;
            if (string.IsNullOrWhiteSpace(trimmedPrice)) return false;
            if (string.IsNullOrWhiteSpace(trimmedStockQuantity)) return false;
            if (string.IsNullOrWhiteSpace(trimmedArticleNumber)) return false;

            if (!decimal.TryParse(trimmedPrice, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal priceValue)) return false;
            if (priceValue < 0) return false;

            if (!int.TryParse(trimmedStockQuantity, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out int stockQuantityValue)) return false;
            if (stockQuantityValue < 0) return false;

            return true;
        }
    }

    public AdminProductsViewModel(ProductService productService, NavigationService navigationService)
    {
        _productService = productService;
        _navigationService = navigationService;
        _services = Services.ServiceLocator.Instance;
        
        Products = new ObservableCollection<Product>();
        LoadCategoriesAndManufacturers();
        
        var dispatcher = System.Windows.Application.Current?.Dispatcher ?? System.Windows.Threading.Dispatcher.CurrentDispatcher;
        dispatcher.BeginInvoke(new Action(async () =>
        {
            try
            {
                await LoadProductsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при загрузке товаров:\n{ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }), System.Windows.Threading.DispatcherPriority.Loaded);
    }

    private async void LoadCategoriesAndManufacturers()
    {
        try
        {
            var categories = await _services.CategoryRepository.GetAllAsync();
            Categories = new ObservableCollection<Category>(categories);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading categories: {ex.Message}");
        }
    }

    partial void OnSelectedCategoryChanged(Category? value)
    {
        if (value != null)
        {
            LoadManufacturersForCategory(value.Id);
        }
        else
        {
            Manufacturers.Clear();
        }
        OnPropertyChanged(nameof(CanSave));
        UpdateIsSaveEnabled();
    }
    
    partial void OnProductNameChanged(string value)
    {
        OnPropertyChanged(nameof(CanSave));
        UpdateIsSaveEnabled();
    }
    
    partial void OnDescriptionChanged(string value)
    {
        OnPropertyChanged(nameof(CanSave));
        UpdateIsSaveEnabled();
    }
    
    partial void OnPriceChanged(string value)
    {
        OnPropertyChanged(nameof(CanSave));
        UpdateIsSaveEnabled();
    }
    
    partial void OnStockQuantityChanged(string value)
    {
        OnPropertyChanged(nameof(CanSave));
        UpdateIsSaveEnabled();
    }
    
    partial void OnArticleNumberChanged(string value)
    {
        OnPropertyChanged(nameof(CanSave));
        UpdateIsSaveEnabled();
    }
    
    partial void OnSelectedManufacturerChanged(Manufacturer? value)
    {
        OnPropertyChanged(nameof(CanSave));
        UpdateIsSaveEnabled();
    }

    private void UpdateIsSaveEnabled()
    {
        IsSaveEnabled = CanSave;
        OnPropertyChanged(nameof(IsSaveEnabled));
    }

    private async void LoadManufacturersForCategory(int categoryId)
    {
        try
        {
            var manufacturers = await _services.ManufacturerRepository.GetByCategoryIdAsync(categoryId);
            Manufacturers = new ObservableCollection<Manufacturer>(manufacturers);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading manufacturers: {ex.Message}");
        }
    }
    
    public async Task LoadProductsAsync()
    {
        try
        {
            var allProducts = await _productService.GetAllProductsAsync();
            Products = new ObservableCollection<Product>(allProducts);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Ошибка при загрузке товаров:\n{ex.Message}",
                "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            Products = new ObservableCollection<Product>();
        }
    }

    [RelayCommand]
    private void AddNewProduct()
    {
        IsEditMode = false;
        SelectedProduct = null;
        ResetForm();
        IsFormVisible = true;
    }

    [RelayCommand]
    private async Task EditProduct(Product product)
    {
        if (product == null) return;
        
        SelectedProduct = product;
        IsEditMode = true;
        IsFormVisible = true;
        
        ProductName = product.Name;
        Description = product.Description;
        Price = product.Price.ToString(System.Globalization.CultureInfo.InvariantCulture);
        StockQuantity = product.StockQuantity.ToString();
        SelectedCategory = product.Category;
        
        if (product.Category != null)
        {
            await LoadManufacturersForCategoryAsync(product.Category.Id);
            SelectedManufacturer = product.Manufacturer;
        }
        else
        {
            SelectedManufacturer = null;
        }
        
        ArticleNumber = product.ArticleNumber;
        ImagePath = product.ImagePath;
        ImageData = product.ImageData;
        IsAvailable = product.IsAvailable;
        
        if (product.ImageData != null && product.ImageData.Length > 0)
        {
            LoadPreviewFromBytes(product.ImageData);
        }
        else if (!string.IsNullOrEmpty(product.ImagePath))
        {
            LoadPreviewFromPath(product.ImagePath);
        }
        else
        {
            PreviewImage = null;
        }
        
        // Принудительно обновляем валидацию после загрузки всех данных
        // Используем Dispatcher для гарантированного обновления после всех изменений свойств
        var dispatcher = System.Windows.Application.Current?.Dispatcher ?? System.Windows.Threading.Dispatcher.CurrentDispatcher;
        _ = dispatcher.BeginInvoke(new Action(() =>
        {
            OnPropertyChanged(nameof(CanSave));
            UpdateIsSaveEnabled();
        }), System.Windows.Threading.DispatcherPriority.Loaded);
    }

    private async Task LoadManufacturersForCategoryAsync(int categoryId)
    {
        try
        {
            var manufacturers = await _services.ManufacturerRepository.GetByCategoryIdAsync(categoryId);
            Manufacturers = new ObservableCollection<Manufacturer>(manufacturers);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading manufacturers: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task SaveProduct()
    {
        // КРИТИЧЕСКАЯ ПРОВЕРКА В САМОМ НАЧАЛЕ - получаем актуальные значения полей напрямую из свойств
        var trimmedProductName = (ProductName ?? string.Empty).Trim();
        var trimmedDescription = (Description ?? string.Empty).Trim();
        var trimmedPrice = (Price ?? string.Empty).Trim();
        var trimmedStockQuantity = (StockQuantity ?? string.Empty).Trim();
        var trimmedArticleNumber = (ArticleNumber ?? string.Empty).Trim();
        
        // КРИТИЧЕСКАЯ ПРОВЕРКА - блокируем сохранение если хотя бы одно поле пустое
        // Проверяем каждое поле отдельно для более точного сообщения об ошибке
        if (string.IsNullOrWhiteSpace(trimmedProductName))
        {
            ShowValidationError("Поле 'Название товара' обязательно должно быть заполнено.");
            return;
        }
        
        if (string.IsNullOrWhiteSpace(trimmedDescription))
        {
            ShowValidationError("Поле 'Описание' обязательно должно быть заполнено.");
            return;
        }
        
        if (SelectedCategory == null)
        {
            ShowValidationError("Поле 'Категория' обязательно должно быть заполнено.");
            return;
        }

        if (SelectedManufacturer == null)
        {
            ShowValidationError("Поле 'Производитель' обязательно должно быть заполнено.");
            return;
        }
        
        if (string.IsNullOrWhiteSpace(trimmedPrice))
        {
            ShowValidationError("Поле 'Цена' обязательно должно быть заполнено.");
            return;
        }

        if (string.IsNullOrWhiteSpace(trimmedStockQuantity))
        {
            ShowValidationError("Поле 'Количество на складе' обязательно должно быть заполнено.");
            return;
        }

        if (string.IsNullOrWhiteSpace(trimmedArticleNumber))
        {
            ShowValidationError("Поле 'Артикул' обязательно должно быть заполнено.");
            return;
        }

        // Проверка формата и значений для цены - СТРОГАЯ ПРОВЕРКА

        if (!decimal.TryParse(trimmedPrice, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal priceValue))
        {
            ShowValidationError("Некорректное значение поля 'Цена'. Введите корректное числовое значение (например: 100.50).");
            return;
        }

        if (priceValue < 0)
        {
            ShowValidationError("Цена не может быть отрицательной.");
            return;
        }

        // Проверка формата и значений для количества - СТРОГАЯ ПРОВЕРКА

        if (!int.TryParse(trimmedStockQuantity, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out int stockQuantityValue))
        {
            ShowValidationError("Некорректное значение поля 'Количество на складе'. Введите корректное целое число (например: 10).");
            return;
        }

        if (stockQuantityValue < 0)
        {
            ShowValidationError("Количество товара на складе не может быть отрицательным.");
            return;
        }
        
        // ФИНАЛЬНАЯ ПРОВЕРКА ПЕРЕД СОХРАНЕНИЕМ - гарантируем, что все поля заполнены
        if (string.IsNullOrWhiteSpace(trimmedProductName) ||
            string.IsNullOrWhiteSpace(trimmedDescription) ||
            SelectedCategory == null ||
            SelectedManufacturer == null ||
            string.IsNullOrWhiteSpace(trimmedPrice) ||
            string.IsNullOrWhiteSpace(trimmedStockQuantity) ||
            string.IsNullOrWhiteSpace(trimmedArticleNumber))
        {
            ShowValidationError("Не все обязательные поля заполнены. Пожалуйста, заполните все поля перед сохранением.");
            return;
        }

        // Сохранение товара
        try
        {
            if (IsEditMode && SelectedProduct != null)
            {
                // Редактирование существующего товара
                SelectedProduct.Name = trimmedProductName;
                SelectedProduct.Description = trimmedDescription;
                SelectedProduct.Price = priceValue;
                SelectedProduct.StockQuantity = stockQuantityValue;
                SelectedProduct.CategoryId = SelectedCategory.Id;
                SelectedProduct.Category = SelectedCategory;
                SelectedProduct.ManufacturerId = SelectedManufacturer.Id;
                SelectedProduct.Manufacturer = SelectedManufacturer;
                SelectedProduct.ArticleNumber = trimmedArticleNumber;
                SelectedProduct.ImagePath = ImagePath?.Trim() ?? string.Empty;
                SelectedProduct.ImageData = ImageData;
                SelectedProduct.IsAvailable = IsAvailable;

                await _productService.UpdateProductAsync(SelectedProduct);
                MessageBox.Show("Товар успешно обновлен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // Добавление нового товара
                var newProduct = new Product
                {
                    Name = trimmedProductName,
                    Description = trimmedDescription,
                    Price = priceValue,
                    StockQuantity = stockQuantityValue,
                    CategoryId = SelectedCategory.Id,
                    Category = SelectedCategory,
                    ManufacturerId = SelectedManufacturer.Id,
                    Manufacturer = SelectedManufacturer,
                    ArticleNumber = trimmedArticleNumber,
                    ImagePath = ImagePath?.Trim() ?? string.Empty,
                    ImageData = ImageData,
                    IsAvailable = IsAvailable,
                    CreatedAt = DateTime.Now
                };

                var success = await _productService.AddProductAsync(newProduct);
                
                if (success)
                {
                    MessageBox.Show("Товар успешно добавлен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Не удалось добавить товар. Проверьте данные и попробуйте снова.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            await LoadProductsAsync();
            ResetForm();
            IsFormVisible = false;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ShowValidationError(string message)
    {
        MessageBox.Show(
            $"{message}\n\nПожалуйста, заполните все обязательные поля корректно перед сохранением.",
            "Ошибка валидации",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }

    [RelayCommand]
    private async Task DeleteProduct(Product product)
    {
        if (product == null) return;

        var result = MessageBox.Show(
            $"Вы уверены, что хотите удалить товар '{product.Name}'?",
            "Подтверждение",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                await _productService.DeleteProductAsync(product.Id);
                MessageBox.Show("Товар успешно удален", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadProductsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void ResetForm()
    {
        ProductName = string.Empty;
        Description = string.Empty;
        Price = string.Empty;
        StockQuantity = string.Empty;
        SelectedCategory = null;
        SelectedManufacturer = null;
        Manufacturers.Clear();
        ArticleNumber = string.Empty;
        ImagePath = string.Empty;
        ImageData = null;
        PreviewImage = null;
        IsAvailable = true;
        OnPropertyChanged(nameof(CanSave));
        UpdateIsSaveEnabled();
    }

    [RelayCommand]
    private void LoadImage()
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "Image files (*.jpg;*.jpeg;*.png;*.bmp;*.gif)|*.jpg;*.jpeg;*.png;*.bmp;*.gif|All files (*.*)|*.*",
            Title = "Выберите изображение товара"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            try
            {
                var filePath = openFileDialog.FileName;
                ImagePath = Path.GetFileName(filePath);
                ImageData = File.ReadAllBytes(filePath);
                LoadPreviewFromBytes(ImageData);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке изображения:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    [RelayCommand]
    private void ClearImage()
    {
        ImagePath = string.Empty;
        ImageData = null;
        PreviewImage = null;
    }

    public void HandleDrop(string[] filePaths)
    {
        if (filePaths == null || filePaths.Length == 0) return;

        var imageFile = filePaths.FirstOrDefault(f => 
            f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
            f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
            f.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
            f.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase) ||
            f.EndsWith(".gif", StringComparison.OrdinalIgnoreCase));

        if (imageFile != null)
        {
            try
            {
                ImagePath = Path.GetFileName(imageFile);
                ImageData = File.ReadAllBytes(imageFile);
                LoadPreviewFromBytes(ImageData);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке изображения:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void LoadPreviewFromBytes(byte[] data)
    {
        try
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = new MemoryStream(data);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();
            PreviewImage = bitmap;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading preview from bytes: {ex.Message}");
            PreviewImage = null;
        }
    }

    private void LoadPreviewFromPath(string path)
    {
        try
        {
            if (!Path.IsPathRooted(path))
            {
                string basePath = AppDomain.CurrentDomain.BaseDirectory;
                path = Path.Combine(basePath, "Images", path);
            }

            if (File.Exists(path))
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(path, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                PreviewImage = bitmap;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading preview from path: {ex.Message}");
            PreviewImage = null;
        }
    }
}
