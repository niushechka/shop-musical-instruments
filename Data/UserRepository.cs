using Microsoft.EntityFrameworkCore;
using course_project_shop_musical_instruments.Models;

namespace course_project_shop_musical_instruments.Data;

public class UserRepository : IRepository<User>
{
    private readonly DbContextFactory _contextFactory;

    public UserRepository(DbContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<User>> GetAllAsync()
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.Users
            .AsNoTracking()
            .OrderBy(u => u.Id)
            .ToListAsync();
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<int> AddAsync(User user)
    {
        using var context = _contextFactory.CreateDbContext();
        user.CreatedAt = DateTime.Now;
        
        context.Users.Add(user);
        await context.SaveChangesAsync();
        
        return user.Id;
    }

    public async Task<bool> UpdateAsync(User user)
    {
        using var context = _contextFactory.CreateDbContext();
        var existingUser = await context.Users.FindAsync(user.Id);
        if (existingUser == null)
            return false;

        existingUser.Email = user.Email;
        existingUser.PasswordHash = user.PasswordHash;
        existingUser.FirstName = user.FirstName;
        existingUser.LastName = user.LastName;
        existingUser.Phone = user.Phone;
        existingUser.Address = user.Address;
        existingUser.Role = user.Role;

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        var user = await context.Users.FindAsync(id);
        if (user == null)
            return false;

        context.Users.Remove(user);
        await context.SaveChangesAsync();
        
        return true;
    }
}
