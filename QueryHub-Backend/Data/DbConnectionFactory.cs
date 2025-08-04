using Microsoft.Data.Sqlite;
using QueryHub_Backend.Interfaces;

namespace QueryHub_Backend.Data
{
    public class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public DbConnectionFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? "Data Source=QueryHub.db";
        }

        public SqliteConnection CreateConnection()
        {
            return new SqliteConnection(_connectionString);
        }

        public async Task<SqliteConnection> CreateConnectionAsync()
        {
            var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            return connection;
        }
    }
}
