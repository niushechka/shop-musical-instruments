using Microsoft.EntityFrameworkCore;
using course_project_shop_musical_instruments.Models;

namespace course_project_shop_musical_instruments.Data;

public class ProductRepository : IRepository<Product>
{
    private readonly DbContextFactory _contextFactory;

    public ProductRepository(DbContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Product>> GetAllAsync()
    {
        using var context = _contextFactory.CreateDbContext();
        // Убрали фильтрацию по IsDeleted, так как колонка может отсутствовать в существующей БД
        return await context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Manufacturer)
            .OrderBy(p => p.Id)
            .ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Manufacturer)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<Product>> GetByIdsAsync(List<int> ids)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Manufacturer)
            .Where(p => ids.Contains(p.Id))
            .ToListAsync();
    }

    public async Task<int> AddAsync(Product product)
    {
        using var context = _contextFactory.CreateDbContext();
        product.CreatedAt = DateTime.Now;
        product.IsDeleted = false;
        
        context.Products.Add(product);
        await context.SaveChangesAsync();
        
        return product.Id;
    }

    public async Task<bool> UpdateAsync(Product product)
    {
        using var context = _contextFactory.CreateDbContext();
        var existingProduct = await context.Products.FindAsync(product.Id);
        if (existingProduct == null || existingProduct.IsDeleted)
            return false;

        existingProduct.Name = product.Name;
        existingProduct.Description = product.Description;
        existingProduct.Price = product.Price;
        existingProduct.StockQuantity = product.StockQuantity;
        existingProduct.CategoryId = product.CategoryId;
        existingProduct.ManufacturerId = product.ManufacturerId;
        existingProduct.ArticleNumber = product.ArticleNumber;
        existingProduct.ImagePath = product.ImagePath;
        existingProduct.ImageData = product.ImageData;
        existingProduct.IsAvailable = product.IsAvailable;

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        var product = await context.Products.FindAsync(id);
        if (product == null)
            return false;

        // Физическое удаление товара
        context.Products.Remove(product);
        await context.SaveChangesAsync();
        
        return true;
    }
}
