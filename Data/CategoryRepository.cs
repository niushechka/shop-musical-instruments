using Microsoft.EntityFrameworkCore;
using course_project_shop_musical_instruments.Models;

namespace course_project_shop_musical_instruments.Data;

public class CategoryRepository
{
    private readonly DbContextFactory _contextFactory;

    public CategoryRepository(DbContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Category>> GetAllAsync()
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
    }
}
