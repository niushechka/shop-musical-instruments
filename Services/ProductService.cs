using course_project_shop_musical_instruments.Data;
using course_project_shop_musical_instruments.Models;

namespace course_project_shop_musical_instruments.Services;

public class ProductService
{
    private readonly ProductRepository _productRepository;

    public ProductService(ProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<List<Product>> GetAllProductsAsync()
    {
        return await _productRepository.GetAllAsync();
    }

    public async Task<List<Product>> GetProductsByCategoryAsync(string category)
    {
        var allProducts = await _productRepository.GetAllAsync();
        return allProducts.Where(p => p.CategoryName == category && p.IsAvailable).ToList();
    }

    public async Task<List<Product>> SearchProductsAsync(string searchTerm)
    {
        var allProducts = await _productRepository.GetAllAsync();
        var term = searchTerm.ToLower();
        return allProducts.Where(p => 
            p.Name.ToLower().Contains(term) ||
            p.Description.ToLower().Contains(term) ||
            p.ManufacturerName.ToLower().Contains(term) ||
            p.CategoryName.ToLower().Contains(term)
        ).ToList();
    }

    public async Task<List<Product>> FilterProductsAsync(string? category = null, string? manufacturer = null, decimal? minPrice = null, decimal? maxPrice = null)
    {
        var products = await _productRepository.GetAllAsync();
        
        if (category != null)
            products = products.Where(p => p.CategoryName == category).ToList();
        
        if (manufacturer != null)
            products = products.Where(p => p.ManufacturerName == manufacturer).ToList();
        
        if (minPrice.HasValue)
            products = products.Where(p => p.Price >= minPrice.Value).ToList();
        
        if (maxPrice.HasValue)
            products = products.Where(p => p.Price <= maxPrice.Value).ToList();
        
        return products.Where(p => p.IsAvailable).ToList();
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        return await _productRepository.GetByIdAsync(id);
    }

    public async Task<bool> AddProductAsync(Product product)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"ProductService: Adding product '{product.Name}'");
            var id = await _productRepository.AddAsync(product);
            System.Diagnostics.Debug.WriteLine($"ProductService: Product added with ID: {id}");
            return id > 0;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ProductService AddProductAsync ERROR: {ex.Message}\n{ex.StackTrace}");
            throw; // Пробрасываем исключение, чтобы ViewModel мог его обработать
        }
    }

    public async Task<bool> UpdateProductAsync(Product product)
    {
        return await _productRepository.UpdateAsync(product);
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        return await _productRepository.DeleteAsync(id);
    }
}

