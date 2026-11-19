using Microsoft.EntityFrameworkCore;
using course_project_shop_musical_instruments.Models;

namespace course_project_shop_musical_instruments.Data;

public class OrderRepository : IRepository<Order>
{
    private readonly DbContextFactory _contextFactory;

    public OrderRepository(DbContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Order>> GetAllAsync()
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.Orders
            .AsNoTracking()
            .Include(o => o.User)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<List<Order>> GetByUserIdAsync(int userId)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.Orders
            .AsNoTracking()
            .Include(o => o.User)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.Orders
            .AsNoTracking()
            .Include(o => o.User)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<int> AddAsync(Order order)
    {
        using var context = _contextFactory.CreateDbContext();
        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            order.OrderDate = DateTime.Now;
            
            // Загружаем все товары заранее с отслеживанием для обновления
            var productIds = order.OrderItems.Select(oi => oi.ProductId).Distinct().ToList();
            var products = await context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();

            context.Orders.Add(order);
            await context.SaveChangesAsync();

            // Уменьшаем количество товара на складе
            foreach (var item in order.OrderItems)
            {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product != null)
                {
                    product.StockQuantity -= item.Quantity;
                    if (product.StockQuantity <= 0)
                    {
                        product.IsAvailable = false;
                    }
                }
            }

            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            
            return order.Id;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> UpdateAsync(Order order)
    {
        using var context = _contextFactory.CreateDbContext();
        var existingOrder = await context.Orders.FindAsync(order.Id);
        if (existingOrder == null)
            return false;

        existingOrder.Status = order.Status;
        existingOrder.DeliveryAddress = order.DeliveryAddress;
        existingOrder.Notes = order.Notes;

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var order = await context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);
            
            if (order == null)
                return false;

            // Удаляем элементы заказа (каскадное удаление)
            context.OrderItems.RemoveRange(order.OrderItems);
            
            // Удаляем заказ
            context.Orders.Remove(order);
            
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            return false;
        }
    }
}
