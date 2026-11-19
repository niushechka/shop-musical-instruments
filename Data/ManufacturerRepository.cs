using Microsoft.EntityFrameworkCore;
using course_project_shop_musical_instruments.Models;

namespace course_project_shop_musical_instruments.Data;

public class ManufacturerRepository
{
    private readonly DbContextFactory _contextFactory;

    public ManufacturerRepository(DbContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Manufacturer>> GetAllAsync()
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.Manufacturers
            .AsNoTracking()
            .Include(m => m.Category)
            .OrderBy(m => m.Name)
            .ToListAsync();
    }

    public async Task<List<Manufacturer>> GetByCategoryIdAsync(int categoryId)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.Manufacturers
            .AsNoTracking()
            .Include(m => m.Category)
            .Where(m => m.CategoryId == categoryId)
            .OrderBy(m => m.Name)
            .ToListAsync();
    }

    public async Task<Manufacturer?> GetByIdAsync(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.Manufacturers
            .AsNoTracking()
            .Include(m => m.Category)
            .FirstOrDefaultAsync(m => m.Id == id);
    }
}
