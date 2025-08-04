using Microsoft.Data.Sqlite;

namespace QueryHub_Backend.Interfaces
{
    public interface IDbConnectionFactory
    {
        SqliteConnection CreateConnection();
        Task<SqliteConnection> CreateConnectionAsync();
    }
    
    public interface IDatabaseInitializer
    {
        Task InitializeAsync();
        Task SeedDataAsync();
    }
}
