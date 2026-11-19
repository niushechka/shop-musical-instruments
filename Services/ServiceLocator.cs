using System;
using System.Windows;
using course_project_shop_musical_instruments.Data;

namespace course_project_shop_musical_instruments.Services;

public class ServiceLocator
{
    private static ServiceLocator? _instance;
    public static ServiceLocator Instance => _instance ??= new ServiceLocator();

    private readonly DbContextFactory _dbContextFactory;
    private readonly ApplicationDbContext _dbContext; // Для инициализации БД
    private readonly ProductRepository _productRepository;
    private readonly UserRepository _userRepository;
    private readonly OrderRepository _orderRepository;
    private readonly ProductService _productService;
    private readonly OrderService _orderService;
    private readonly AuthService _authService;
    private readonly CartService _cartService;
    private readonly NavigationService _navigationService;
    private readonly EmailService _emailService;

    private ServiceLocator()
    {
        try
        {
            _dbContextFactory = new DbContextFactory();
            _dbContext = new ApplicationDbContext(); // Для инициализации БД при старте
            
            // Создаем репозитории с фабрикой для создания новых контекстов
            _productRepository = new ProductRepository(_dbContextFactory);
            _userRepository = new UserRepository(_dbContextFactory);
            _orderRepository = new OrderRepository(_dbContextFactory);
            _productService = new ProductService(_productRepository);
            _emailService = new EmailService();
            _orderService = new OrderService(_orderRepository, _productRepository, _userRepository, _emailService);
            _authService = new AuthService(_userRepository);
            _cartService = new CartService();
            _navigationService = new NavigationService();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                $"Критическая ошибка при инициализации приложения:\n{ex.Message}\n\n{ex.StackTrace}",
                "Ошибка",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
            throw;
        }
    }

    public ApplicationDbContext DbContext => _dbContext;
    public DbContextFactory DbContextFactory => _dbContextFactory;
    public ProductRepository ProductRepository => _productRepository;
    public UserRepository UserRepository => _userRepository;
    public OrderRepository OrderRepository => _orderRepository;
    public ProductService ProductService => _productService;
    public OrderService OrderService => _orderService;
    public AuthService AuthService => _authService;
    public CartService CartService => _cartService;
    public NavigationService NavigationService => _navigationService;
    public EmailService EmailService => _emailService;
    
    private CategoryRepository? _categoryRepository;
    private ManufacturerRepository? _manufacturerRepository;
    
    public CategoryRepository CategoryRepository => _categoryRepository ??= new CategoryRepository(_dbContextFactory);
    public ManufacturerRepository ManufacturerRepository => _manufacturerRepository ??= new ManufacturerRepository(_dbContextFactory);
}

