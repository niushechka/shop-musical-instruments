using course_project_shop_musical_instruments.Data;
using course_project_shop_musical_instruments.Models;

namespace course_project_shop_musical_instruments.Services;

public class OrderService
{
    private readonly OrderRepository _orderRepository;
    private readonly ProductRepository _productRepository;
    private readonly UserRepository _userRepository;
    private readonly EmailService _emailService;

    public OrderService(OrderRepository orderRepository, ProductRepository productRepository, UserRepository userRepository, EmailService emailService)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _userRepository = userRepository;
        _emailService = emailService;
    }

    public async Task<List<Order>> GetAllOrdersAsync()
    {
        return await _orderRepository.GetAllAsync();
    }

    public async Task<List<Order>> GetUserOrdersAsync(int userId)
    {
        return await _orderRepository.GetByUserIdAsync(userId);
    }

    public async Task<Order?> GetOrderByIdAsync(int id)
    {
        return await _orderRepository.GetByIdAsync(id);
    }

    public async Task<int> CreateOrderAsync(int userId, List<CartItem> cartItems, DateTime deliveryDate, TimeSpan deliveryTime, string deliveryAddress, string? notes = null)
    {
        var order = new Order
        {
            UserId = userId,
            OrderDate = DateTime.Now,
            DeliveryDate = deliveryDate,
            DeliveryTime = deliveryTime,
            DeliveryAddress = deliveryAddress,
            Notes = notes,
            Status = OrderStatus.Pending
        };

        // Загружаем все товары одним запросом, чтобы избежать проблем с DataReader
        var productIds = cartItems.Select(ci => ci.ProductId).Distinct().ToList();
        var products = await _productRepository.GetByIdsAsync(productIds);

        // Проверяем доступность товаров перед созданием заказа
        foreach (var cartItem in cartItems)
        {
            var product = products.FirstOrDefault(p => p.Id == cartItem.ProductId);
            if (product != null)
            {
                // Проверяем, что товара достаточно на складе
                if (product.StockQuantity < cartItem.Quantity)
                {
                    throw new Exception($"Недостаточно товара '{product.Name}' на складе. Доступно: {product.StockQuantity}, требуется: {cartItem.Quantity}");
                }

                order.OrderItems.Add(new OrderItem
                {
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = product.Price
                });
            }
        }

        order.TotalAmount = order.OrderItems.Sum(item => item.Subtotal);

        // Создаем заказ (количество товаров уменьшится автоматически в транзакции)
        var orderId = await _orderRepository.AddAsync(order);
        
        System.Diagnostics.Debug.WriteLine($"[OrderService] Заказ #{orderId} создан успешно. Начинаем отправку email...");

        // Отправляем email-уведомление о заказе
        try
        {
            System.Diagnostics.Debug.WriteLine($"[OrderService] Загрузка заказа #{orderId} для отправки email...");
            var createdOrder = await _orderRepository.GetByIdAsync(orderId);
            
            if (createdOrder == null)
            {
                System.Diagnostics.Debug.WriteLine($"[OrderService] ОШИБКА: Заказ #{orderId} не найден после создания!");
                return orderId;
            }
            
            System.Diagnostics.Debug.WriteLine($"[OrderService] Заказ #{orderId} загружен. UserId: {createdOrder.UserId}");
            
            if (createdOrder.User == null)
            {
                System.Diagnostics.Debug.WriteLine($"[OrderService] ВНИМАНИЕ: User не загружен с заказом. Загружаем отдельно...");
                // Загружаем пользователя отдельно, если он не загрузился
                var user = await _userRepository.GetByIdAsync(createdOrder.UserId);
                if (user == null)
                {
                    System.Diagnostics.Debug.WriteLine($"[OrderService] ОШИБКА: Пользователь с ID {createdOrder.UserId} не найден!");
                    return orderId;
                }
                createdOrder.User = user;
            }
            
            System.Diagnostics.Debug.WriteLine($"[OrderService] Пользователь найден: {createdOrder.User.Email}. Начинаем отправку email...");
            
            // Отправляем email в фоновом потоке с обработкой ошибок
            var emailTask = Task.Run(async () =>
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"[OrderService] [Background Task] Начало отправки email для заказа #{orderId}");
                    System.Diagnostics.Debug.WriteLine($"[OrderService] [Background Task] EmailService создан: {_emailService != null}");
                    
                    var emailSent = await _emailService.SendOrderConfirmationEmailAsync(createdOrder, createdOrder.User!);
                    
                    System.Diagnostics.Debug.WriteLine($"[OrderService] [Background Task] Результат отправки: {emailSent}");
                    
                    if (!emailSent)
                    {
                        System.Diagnostics.Debug.WriteLine($"[OrderService] [Background Task] ВНИМАНИЕ: Email для заказа #{orderId} не был отправлен. Проверьте логи в файле email_log.txt");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[OrderService] [Background Task] Email для заказа #{orderId} успешно отправлен!");
                    }
                }
                catch (Exception ex)
                {
                    // Логируем ошибку отправки email, но не прерываем процесс
                    System.Diagnostics.Debug.WriteLine($"[OrderService] [Background Task] ОШИБКА отправки email для заказа #{orderId}: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"[OrderService] [Background Task] StackTrace: {ex.StackTrace}");
                    System.Diagnostics.Debug.WriteLine($"[OrderService] [Background Task] Тип ошибки: {ex.GetType().Name}");
                }
            });
            
            // Сохраняем ссылку на задачу, чтобы она не была собрана сборщиком мусора
            // и выполнялась до завершения
            System.Diagnostics.Debug.WriteLine($"[OrderService] Задача отправки email запущена в фоне для заказа #{orderId}. Task ID: {emailTask.Id}");
            
            // Опционально: можно дождаться завершения задачи (но это заблокирует UI)
            // await emailTask;
        }
        catch (Exception ex)
        {
            // Логируем ошибку, но не прерываем процесс оформления заказа
            System.Diagnostics.Debug.WriteLine($"[OrderService] ОШИБКА при подготовке отправки email для заказа #{orderId}: {ex.Message}\n{ex.StackTrace}");
        }

        return orderId;
    }

    public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null) return false;

        order.Status = status;
        return await _orderRepository.UpdateAsync(order);
    }

    public async Task<bool> DeleteOrderAsync(int id)
    {
        return await _orderRepository.DeleteAsync(id);
    }
}

