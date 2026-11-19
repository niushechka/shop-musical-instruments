using Microsoft.Data.SqlClient;
using System.Data;
using System.Collections.Generic;

namespace course_project_shop_musical_instruments.Data;

public class DatabaseContext
{
    private readonly string _connectionString;
    private readonly string _masterConnectionString;
    private readonly string _databaseName = "MusicShopDB";

    private bool _initialized = false;
    private readonly object _lockObject = new object();

    public DatabaseContext()
    {
        var server = "Niusha";
        _masterConnectionString = $"Server={server};Database=master;Integrated Security=True;TrustServerCertificate=True;";
        _connectionString = $"Server={server};Database={_databaseName};Integrated Security=True;TrustServerCertificate=True;";
    }

    public void EnsureInitialized()
    {
        if (_initialized) return;

        lock (_lockObject)
        {
            if (_initialized) return;

            try
            {
                InitializeDatabase();
                _initialized = true;
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

    private void InitializeDatabase()
    {
        // Сначала создаем базу данных, если её нет
        CreateDatabaseIfNotExists();

        // Затем создаем таблицы
        CreateTables();
    }

    private void CreateDatabaseIfNotExists()
    {
        try
        {
            using var connection = new SqlConnection(_masterConnectionString);
            connection.Open();

            var checkDbCommand = new SqlCommand(
                $"SELECT COUNT(*) FROM sys.databases WHERE name = '{_databaseName}'", 
                connection);
            
            var databaseExists = (int)checkDbCommand.ExecuteScalar() > 0;

            if (!databaseExists)
            {
                // Получаем путь к данным SQL Server
                var getDataPathCommand = new SqlCommand(
                    "SELECT SUBSTRING(physical_name, 1, CHARINDEX(N'master.mdf', LOWER(physical_name)) - 1) " +
                    "FROM master.sys.master_files WHERE database_id = 1 AND file_id = 1",
                    connection);
                
                var dataPath = getDataPathCommand.ExecuteScalar()?.ToString() ?? "C:\\Program Files\\Microsoft SQL Server\\MSSQL15.MSSQLSERVER\\MSSQL\\DATA\\";
                
                var createDbCommand = new SqlCommand(
                    $@"CREATE DATABASE [{_databaseName}] 
                       ON (NAME = '{_databaseName}', 
                           FILENAME = '{dataPath}{_databaseName}.mdf')
                       LOG ON (NAME = '{_databaseName}_Log', 
                              FILENAME = '{dataPath}{_databaseName}_Log.ldf')", 
                    connection);
                createDbCommand.ExecuteNonQuery();
            }
        }
        catch (SqlException sqlEx)
        {
            throw new Exception($"Ошибка при создании базы данных: {sqlEx.Message}", sqlEx);
        }
    }

    private void CreateTables()
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        // Create Users table
        var createUsersTable = @"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
            CREATE TABLE [dbo].[Users] (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                Email NVARCHAR(255) UNIQUE NOT NULL,
                PasswordHash NVARCHAR(255) NOT NULL,
                FirstName NVARCHAR(100) NOT NULL,
                LastName NVARCHAR(100) NOT NULL,
                Phone NVARCHAR(50),
                Address NVARCHAR(500),
                Role INT NOT NULL DEFAULT 0,
                CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
            )";

        // Create Categories table
        var createCategoriesTable = @"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Categories]') AND type in (N'U'))
            CREATE TABLE [dbo].[Categories] (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                Name NVARCHAR(100) NOT NULL UNIQUE,
                Description NVARCHAR(500)
            )";

        // Create Manufacturers table
        var createManufacturersTable = @"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Manufacturers]') AND type in (N'U'))
            CREATE TABLE [dbo].[Manufacturers] (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                Name NVARCHAR(100) NOT NULL,
                CategoryId INT NOT NULL,
                FOREIGN KEY (CategoryId) REFERENCES Categories(Id) ON DELETE CASCADE,
                UNIQUE (Name, CategoryId)
            )";

        // Create Products table
        var createProductsTable = @"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Products]') AND type in (N'U'))
            CREATE TABLE [dbo].[Products] (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                Name NVARCHAR(255) NOT NULL,
                Description NVARCHAR(MAX),
                Price DECIMAL(18,2) NOT NULL,
                StockQuantity INT NOT NULL DEFAULT 0,
                CategoryId INT NOT NULL,
                ManufacturerId INT NOT NULL,
                ArticleNumber NVARCHAR(50),
                ImagePath NVARCHAR(500),
                ImageData VARBINARY(MAX),
                IsAvailable BIT NOT NULL DEFAULT 1,
                IsDeleted BIT NOT NULL DEFAULT 0,
                CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
                FOREIGN KEY (CategoryId) REFERENCES Categories(Id),
                FOREIGN KEY (ManufacturerId) REFERENCES Manufacturers(Id)
            )";

        // Execute table creation
        ExecuteCommand(connection, createUsersTable);
        ExecuteCommand(connection, createCategoriesTable);
        ExecuteCommand(connection, createManufacturersTable);
        ExecuteCommand(connection, createProductsTable);

        // Migrate existing Products table if needed
        MigrateProductsTable(connection);

        // Create Orders table
        var createOrdersTable = @"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Orders]') AND type in (N'U'))
            CREATE TABLE [dbo].[Orders] (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                UserId INT NOT NULL,
                OrderDate DATETIME NOT NULL DEFAULT GETDATE(),
                DeliveryDate DATE NOT NULL,
                DeliveryTime TIME NOT NULL,
                TotalAmount DECIMAL(18,2) NOT NULL,
                Status INT NOT NULL DEFAULT 0,
                DeliveryAddress NVARCHAR(500) NOT NULL,
                Notes NVARCHAR(MAX),
                FOREIGN KEY (UserId) REFERENCES Users(Id)
            )";

        // Create OrderItems table
        var createOrderItemsTable = @"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OrderItems]') AND type in (N'U'))
            CREATE TABLE [dbo].[OrderItems] (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                OrderId INT NOT NULL,
                ProductId INT NOT NULL,
                Quantity INT NOT NULL,
                UnitPrice DECIMAL(18,2) NOT NULL,
                FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE
            )";
        
        // Добавляем FK для ProductId отдельно с NO ACTION, чтобы можно было удалять товары
        ExecuteCommand(connection, createOrdersTable);
        ExecuteCommand(connection, createOrderItemsTable);
        
        // НЕ добавляем FK для ProductId - это позволит удалять товары без ограничений
        // OrderItems останутся с ProductId, но товар можно будет физически удалить

        // Миграция FK для OrderItems - разрешаем удаление товаров
        MigrateOrderItemsForeignKey(connection);
        
        // Insert default categories and manufacturers
        InsertDefaultCategoriesAndManufacturers(connection);

        // Insert default admin user if not exists
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Users WHERE Role = 1";
        var adminCount = Convert.ToInt32(cmd.ExecuteScalar());
        if (adminCount == 0)
        {
            cmd.CommandText = @"
                INSERT INTO Users (Email, PasswordHash, FirstName, LastName, Role, CreatedAt)
                VALUES ('admin@shop.com', 'admin123', 'Admin', 'User', 1, GETDATE())
            ";
            cmd.ExecuteNonQuery();
        }
    }

    private void ExecuteCommand(SqlConnection connection, string commandText)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = commandText;
        cmd.ExecuteNonQuery();
    }

    private void MigrateProductsTable(SqlConnection connection)
    {
        try
        {
            // Проверяем, существует ли старая таблица Products с колонкой Category (строка)
            var checkColumnCommand = new SqlCommand(@"
                SELECT COUNT(*) 
                FROM INFORMATION_SCHEMA.COLUMNS 
                WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'Category' AND DATA_TYPE = 'nvarchar'
            ", connection);
            
            var hasOldSchema = Convert.ToInt32(checkColumnCommand.ExecuteScalar()) > 0;
            
            if (hasOldSchema)
            {
                // Создаем временную таблицу для миграции
                var migrateCommand = new SqlCommand(@"
                    -- Создаем Categories из существующих данных
                    INSERT INTO Categories (Name)
                    SELECT DISTINCT Category 
                    FROM Products 
                    WHERE Category IS NOT NULL AND Category NOT IN (SELECT Name FROM Categories);
                    
                    -- Создаем Manufacturers из существующих данных
                    INSERT INTO Manufacturers (Name, CategoryId)
                    SELECT DISTINCT p.Manufacturer, c.Id
                    FROM Products p
                    INNER JOIN Categories c ON p.Category = c.Name
                    WHERE p.Manufacturer IS NOT NULL 
                    AND NOT EXISTS (
                        SELECT 1 FROM Manufacturers m 
                        WHERE m.Name = p.Manufacturer AND m.CategoryId = c.Id
                    );
                    
                    -- Добавляем новые колонки если их нет
                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'CategoryId')
                    BEGIN
                        ALTER TABLE Products ADD CategoryId INT;
                        ALTER TABLE Products ADD ManufacturerId INT;
                        ALTER TABLE Products ADD ImageData VARBINARY(MAX);
                    END
                    
                    -- Добавляем колонку IsDeleted если её нет
                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'IsDeleted')
                    BEGIN
                        ALTER TABLE Products ADD IsDeleted BIT NOT NULL DEFAULT 0;
                    END
                    
                    -- Изменяем FK для ProductId в OrderItems, чтобы разрешить удаление товаров
                    -- Сначала удаляем старый FK если он есть (ищем по referenced_object_id)
                    DECLARE @fkName NVARCHAR(128)
                    SELECT @fkName = name FROM sys.foreign_keys 
                    WHERE parent_object_id = OBJECT_ID('OrderItems') 
                    AND referenced_object_id = OBJECT_ID('Products')
                    AND name != 'FK_OrderItems_Products'
                    
                    IF @fkName IS NOT NULL
                    BEGIN
                        DECLARE @sql NVARCHAR(MAX) = 'ALTER TABLE OrderItems DROP CONSTRAINT ' + QUOTENAME(@fkName)
                        EXEC sp_executesql @sql
                    END
                    
                    -- Добавляем новый FK с NO ACTION если его еще нет
                    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_OrderItems_Products' AND parent_object_id = OBJECT_ID('OrderItems'))
                    BEGIN
                        ALTER TABLE OrderItems 
                        ADD CONSTRAINT FK_OrderItems_Products 
                        FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE NO ACTION
                    END
                    
                    -- Обновляем CategoryId и ManufacturerId
                    UPDATE Products 
                    SET CategoryId = c.Id
                    FROM Products p
                    INNER JOIN Categories c ON p.Category = c.Name
                    WHERE Products.Id = p.Id AND Products.CategoryId IS NULL;
                    
                    UPDATE Products 
                    SET ManufacturerId = m.Id
                    FROM Products p
                    INNER JOIN Manufacturers m ON p.Manufacturer = m.Name
                    WHERE Products.Id = p.Id AND Products.ManufacturerId IS NULL;
                    
                    -- Удаляем старые колонки (опционально, можно оставить для обратной совместимости)
                    -- ALTER TABLE Products DROP COLUMN Category;
                    -- ALTER TABLE Products DROP COLUMN Manufacturer;
                ", connection);
                
                migrateCommand.ExecuteNonQuery();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Migration error: {ex.Message}");
            // Продолжаем работу даже если миграция не удалась
        }
    }

    private void MigrateOrderItemsForeignKey(SqlConnection connection)
    {
        try
        {
            // Проверяем, существует ли таблица OrderItems
            var checkTableCommand = new SqlCommand(@"
                SELECT COUNT(*) 
                FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_NAME = 'OrderItems'
            ", connection);
            
            if (Convert.ToInt32(checkTableCommand.ExecuteScalar()) == 0)
            {
                return; // Таблица еще не создана
            }
            
            // Удаляем ВСЕ существующие FK для ProductId в OrderItems
            var findAllFkCommand = new SqlCommand(@"
                SELECT name 
                FROM sys.foreign_keys 
                WHERE parent_object_id = OBJECT_ID('OrderItems') 
                AND referenced_object_id = OBJECT_ID('Products')
            ", connection);
            
            using var reader = findAllFkCommand.ExecuteReader();
            var fkNames = new List<string>();
            while (reader.Read())
            {
                fkNames.Add(reader.GetString(0));
            }
            reader.Close();
            
            // Удаляем все найденные FK
            foreach (var fkName in fkNames)
            {
                try
                {
                    // Используем динамический SQL для безопасного удаления FK
                    var dropFkSql = $"ALTER TABLE [dbo].[OrderItems] DROP CONSTRAINT [{fkName}]";
                    var dropFkCommand = new SqlCommand(dropFkSql, connection);
                    dropFkCommand.ExecuteNonQuery();
                    System.Diagnostics.Debug.WriteLine($"Successfully dropped FK constraint: {fkName}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error dropping FK {fkName}: {ex.Message}\n{ex.StackTrace}");
                    // Пытаемся удалить через другой способ
                    try
                    {
                        var dropFkCommand2 = new SqlCommand($"ALTER TABLE OrderItems DROP CONSTRAINT {fkName}", connection);
                        dropFkCommand2.ExecuteNonQuery();
                        System.Diagnostics.Debug.WriteLine($"Successfully dropped FK constraint (method 2): {fkName}");
                    }
                    catch (Exception ex2)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to drop FK {fkName} with both methods: {ex2.Message}");
                    }
                }
            }
            
            // Проверяем, что все FK удалены
            var verifyCommand = new SqlCommand(@"
                SELECT COUNT(*) 
                FROM sys.foreign_keys 
                WHERE parent_object_id = OBJECT_ID('OrderItems') 
                AND referenced_object_id = OBJECT_ID('Products')
            ", connection);
            var remainingFkCount = Convert.ToInt32(verifyCommand.ExecuteScalar());
            if (remainingFkCount > 0)
            {
                System.Diagnostics.Debug.WriteLine($"Warning: {remainingFkCount} FK constraints still exist after migration attempt");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("All FK constraints successfully removed");
            }
            
            // НЕ создаем новый FK - позволяем удалять товары без ограничений
            // OrderItems останутся с ProductId, но товар можно будет удалить
            System.Diagnostics.Debug.WriteLine("OrderItems FK migration completed - FK removed to allow product deletion");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Warning: Could not migrate OrderItems FK: {ex.Message}\n{ex.StackTrace}");
            // Продолжаем работу даже если миграция не удалась
        }
    }

    private void InsertDefaultCategoriesAndManufacturers(SqlConnection connection)
    {
        try
        {
            using var cmd = connection.CreateCommand();
            
            // Определяем категории и их производителей
            var categories = new Dictionary<string, (string Description, string[] Manufacturers)>
            {
                { "Гитары", ("Акустические и электрогитары", new[] { "Fender", "Gibson", "Yamaha", "Enya"}) },
                { "Клавишные", ("Пианино, синтезаторы, клавиатуры", new[] { "Yamaha", "Roland", "Korg", "KAYSERBURG" }) },
                { "Ударные", ("Барабаны, перкуссия", new[] { "Pearl", "Tama", "Yamaha", "BeatHoven" }) },
                { "Струнные", ("Скрипки, виолончели, контрабасы", new[] { "Mirra", "Lamtoys", "Cremona", "Ibanez" }) },
                { "Духовые", ("Флейты, саксофоны, трубы", new[] { "Yamaha", "Selmer", "Angel" }) }
            };
            
            // Добавляем категории, если их нет
            foreach (var category in categories)
            {
                cmd.CommandText = @"
                    IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = @Name)
                    BEGIN
                        INSERT INTO Categories (Name, Description) VALUES (@Name, @Description);
                    END
                ";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@Name", category.Key);
                cmd.Parameters.AddWithValue("@Description", category.Value.Description);
                cmd.ExecuteNonQuery();
            }
            
            // Добавляем производителей для каждой категории
            foreach (var category in categories)
            {
                // Получаем ID категории
                cmd.CommandText = "SELECT Id FROM Categories WHERE Name = @Name";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@Name", category.Key);
                var categoryId = cmd.ExecuteScalar();
                
                if (categoryId != null && categoryId != DBNull.Value)
                {
                    int catId = Convert.ToInt32(categoryId);
                    
                    // Добавляем производителей, если их нет
                    foreach (var manufacturer in category.Value.Manufacturers)
                    {
                        cmd.CommandText = @"
                            IF NOT EXISTS (SELECT 1 FROM Manufacturers WHERE Name = @ManufacturerName AND CategoryId = @CategoryId)
                            BEGIN
                                INSERT INTO Manufacturers (Name, CategoryId) VALUES (@ManufacturerName, @CategoryId);
                            END
                        ";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@ManufacturerName", manufacturer);
                        cmd.Parameters.AddWithValue("@CategoryId", catId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error inserting default categories: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    public SqlConnection GetConnection() => new SqlConnection(_connectionString);
}
