namespace course_project_shop_musical_instruments.Data;

public class DbContextFactory
{
    private readonly string _connectionString;
    private readonly string _masterConnectionString;
    private readonly string _databaseName = "MusicShopDB";

    public DbContextFactory()
    {
        // Строка подключения к MS SQL Server 2022
        // Измените Server на ваши настройки
        var server = "Niusha";
        _masterConnectionString = $"Server={server};Database=master;Integrated Security=True;TrustServerCertificate=True;";
        _connectionString = $"Server={server};Database={_databaseName};Integrated Security=True;TrustServerCertificate=True;";
    }

    public ApplicationDbContext CreateDbContext()
    {
        return new ApplicationDbContext(_connectionString, _masterConnectionString, _databaseName);
    }
}








