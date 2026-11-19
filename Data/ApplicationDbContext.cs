using Microsoft.EntityFrameworkCore;
using course_project_shop_musical_instruments.Models;

namespace course_project_shop_musical_instruments.Data;

public class ApplicationDbContext : DbContext
{
    private readonly string _connectionString;
    private readonly string _masterConnectionString;
    private readonly string _databaseName = "MusicShopDB";
    private static bool _isDeletedColumnChecked = false;
    private static readonly object _checkLock = new object();

    public ApplicationDbContext()
    {
        // Строка подключения к MS SQL Server 2022
        // Измените Server на ваши настройки
        var server = "Niusha";
        _masterConnectionString = $"Server={server};Database=master;Integrated Security=True;TrustServerCertificate=True;";
        _connectionString = $"Server={server};Database={_databaseName};Integrated Security=True;TrustServerCertificate=True;";
    }

    public ApplicationDbContext(string connectionString, string masterConnectionString, string databaseName)
    {
        _connectionString = connectionString;
        _masterConnectionString = masterConnectionString;
        _databaseName = databaseName;
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Manufacturer> Manufacturers { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderItem> OrderItems { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(_connectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Настройка User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Role).HasConversion<int>();
            // Настройка nullable полей
            entity.Property(e => e.Phone).IsRequired(false);
            entity.Property(e => e.Address).IsRequired(false);
        });

        // Настройка Category
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Настройка Manufacturer
        modelBuilder.Entity<Manufacturer>(entity =>
        {
            entity.HasIndex(e => new { e.Name, e.CategoryId }).IsUnique();
            entity.HasOne(m => m.Category)
                  .WithMany(c => c.Manufacturers)
                  .HasForeignKey(m => m.CategoryId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Настройка Product
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasOne(p => p.Category)
                  .WithMany(c => c.Products)
                  .HasForeignKey(p => p.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(p => p.Manufacturer)
                  .WithMany(m => m.Products)
                  .HasForeignKey(p => p.ManufacturerId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            // Убрали глобальный фильтр, чтобы избежать ошибок с существующей БД
            // Фильтрация по IsDeleted будет выполняться явно в репозиториях
        });

        // Настройка Order
        modelBuilder.Entity<Order>(entity =>
        {
            entity.Property(e => e.Status).HasConversion<int>();
            entity.HasOne(o => o.User)
                  .WithMany(u => u.Orders)
                  .HasForeignKey(o => o.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Настройка OrderItem
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasOne(oi => oi.Order)
                  .WithMany(o => o.OrderItems)
                  .HasForeignKey(oi => oi.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(oi => oi.Product)
                  .WithMany(p => p.OrderItems)
                  .HasForeignKey(oi => oi.ProductId)
                  .OnDelete(DeleteBehavior.NoAction); // Позволяет удалять товары без ограничений
        });

        // Вставка начальных данных
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Категории и производители
        var categories = new List<Category>
        {
            new Category { Id = 1, Name = "Гитары", Description = "Акустические и электрогитары" },
            new Category { Id = 2, Name = "Клавишные", Description = "Пианино, синтезаторы, клавиатуры" },
            new Category { Id = 3, Name = "Ударные", Description = "Барабаны, перкуссия" },
            new Category { Id = 4, Name = "Струнные", Description = "Скрипки, виолончели, контрабасы" },
            new Category { Id = 5, Name = "Духовые", Description = "Флейты, саксофоны, трубы" }
        };

        var manufacturers = new List<Manufacturer>
        {
            new Manufacturer { Id = 1, Name = "Fender", CategoryId = 1 },
            new Manufacturer { Id = 2, Name = "Gibson", CategoryId = 1 },
            new Manufacturer { Id = 3, Name = "Yamaha", CategoryId = 1 },
            new Manufacturer { Id = 4, Name = "Enya", CategoryId = 1 },
            new Manufacturer { Id = 5, Name = "Yamaha", CategoryId = 2 },
            new Manufacturer { Id = 6, Name = "Roland", CategoryId = 2 },
            new Manufacturer { Id = 7, Name = "Korg", CategoryId = 2 },
            new Manufacturer { Id = 8, Name = "KAYSERBURG", CategoryId = 2 },
            new Manufacturer { Id = 9, Name = "Pearl", CategoryId = 3 },
            new Manufacturer { Id = 10, Name = "Tama", CategoryId = 3 },
            new Manufacturer { Id = 11, Name = "Yamaha", CategoryId = 3 },
            new Manufacturer { Id = 12, Name = "BeatHoven", CategoryId = 3 },
            new Manufacturer { Id = 13, Name = "Mirra", CategoryId = 4 },
            new Manufacturer { Id = 14, Name = "Lamtoys", CategoryId = 4 },
            new Manufacturer { Id = 15, Name = "Cremona", CategoryId = 4 },
            new Manufacturer { Id = 16, Name = "Ibanez", CategoryId = 4 },
            new Manufacturer { Id = 17, Name = "Yamaha", CategoryId = 5 },
            new Manufacturer { Id = 18, Name = "Selmer", CategoryId = 5 },
            new Manufacturer { Id = 19, Name = "Angel", CategoryId = 5 }
        };

        modelBuilder.Entity<Category>().HasData(categories);
        modelBuilder.Entity<Manufacturer>().HasData(manufacturers);

        // Администратор по умолчанию
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Email = "admin@shop.com",
                PasswordHash = "admin123",
                FirstName = "Admin",
                LastName = "User",
                Phone = null,
                Address = null,
                Role = UserRole.Admin,
                CreatedAt = DateTime.Now
            }
        );
    }

    public void EnsureDatabaseCreated()
    {
        lock (_checkLock)
        {
            if (_isDeletedColumnChecked)
            {
                // Уже инициализировано, просто проверяем что база создана
                try
                {
                    Database.EnsureCreated();
                }
                catch
                {
                    // Игнорируем ошибки если база уже существует
                }
                return;
            }

            try
            {
                // Проверяем существование базы данных
                using var masterConnection = new Microsoft.Data.SqlClient.SqlConnection(_masterConnectionString);
                masterConnection.Open();

                var checkDbCommand = new Microsoft.Data.SqlClient.SqlCommand(
                    $"SELECT COUNT(*) FROM sys.databases WHERE name = '{_databaseName}'",
                    masterConnection);

                var databaseExists = (int)checkDbCommand.ExecuteScalar() > 0;

                if (!databaseExists)
                {
                    // Создаем базу данных
                    var createDbCommand = new Microsoft.Data.SqlClient.SqlCommand(
                        $"CREATE DATABASE [{_databaseName}]",
                        masterConnection);
                    createDbCommand.ExecuteNonQuery();
                }

                // Применяем миграции или создаем схему
                Database.EnsureCreated();
                
                // Проверяем и добавляем колонку IsDeleted, если её нет (только один раз)
                EnsureIsDeletedColumnExists();
                _isDeletedColumnChecked = true;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Ошибка инициализации базы данных:\n{ex.Message}\n\nУбедитесь, что SQL Server запущен и доступен.",
                    "Ошибка базы данных",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
                throw;
            }
        }
    }

    private void EnsureIsDeletedColumnExists()
    {
        try
        {
            // Используем отдельное подключение, чтобы избежать конфликта с активными запросами
            using var connection = new Microsoft.Data.SqlClient.SqlConnection(_connectionString);
            connection.Open();
            
            using var command = connection.CreateCommand();
            command.CommandText = @"
                IF NOT EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'IsDeleted'
                )
                BEGIN
                    ALTER TABLE Products 
                    ADD IsDeleted BIT NOT NULL DEFAULT 0
                END";
            
            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Warning: Could not ensure IsDeleted column exists: {ex.Message}");
            // Продолжаем работу даже если не удалось добавить колонку
        }
    }
}

