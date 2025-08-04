using Microsoft.Data.Sqlite;
using QueryHub_Backend.Interfaces;
using QueryHub_Backend.Models;
using System.Data;

namespace QueryHub_Backend.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public UserRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, Name, Username, Email, PasswordHash, CreatedAt, Reputation 
                FROM Users 
                WHERE Id = @id";
            command.Parameters.Add(new SqliteParameter("@id", id));

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapUser(reader);
            }

            return null;
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, Name, Username, Email, PasswordHash, CreatedAt, Reputation 
                FROM Users 
                WHERE Username = @username";
            command.Parameters.Add(new SqliteParameter("@username", username));

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapUser(reader);
            }

            return null;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, Name, Username, Email, PasswordHash, CreatedAt, Reputation 
                FROM Users 
                WHERE Email = @email";
            command.Parameters.Add(new SqliteParameter("@email", email));

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapUser(reader);
            }

            return null;
        }

        public async Task<User> CreateAsync(User user)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Users (Name, Username, Email, PasswordHash, CreatedAt, Reputation)
                VALUES (@name, @username, @email, @passwordHash, @createdAt, @reputation);
                SELECT last_insert_rowid();";

            command.Parameters.Add(new SqliteParameter("@name", user.Name));
            command.Parameters.Add(new SqliteParameter("@username", user.Username));
            command.Parameters.Add(new SqliteParameter("@email", user.Email));
            command.Parameters.Add(new SqliteParameter("@passwordHash", user.PasswordHash));
            command.Parameters.Add(new SqliteParameter("@createdAt", user.CreatedAt));
            command.Parameters.Add(new SqliteParameter("@reputation", user.Reputation));

            var id = Convert.ToInt32(await command.ExecuteScalarAsync());
            user.Id = id;

            return user;
        }

        public async Task<User> UpdateAsync(User user)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Users 
                SET Name = @name, Username = @username, Email = @email, PasswordHash = @passwordHash, Reputation = @reputation
                WHERE Id = @id";

            command.Parameters.Add(new SqliteParameter("@id", user.Id));
            command.Parameters.Add(new SqliteParameter("@name", user.Name));
            command.Parameters.Add(new SqliteParameter("@username", user.Username));
            command.Parameters.Add(new SqliteParameter("@email", user.Email));
            command.Parameters.Add(new SqliteParameter("@passwordHash", user.PasswordHash));
            command.Parameters.Add(new SqliteParameter("@reputation", user.Reputation));

            await command.ExecuteNonQueryAsync();
            return user;
        }

        public async Task DeleteAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Users WHERE Id = @id";
            command.Parameters.Add(new SqliteParameter("@id", id));

            await command.ExecuteNonQueryAsync();
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            var users = new List<User>();

            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, Name, Username, Email, PasswordHash, CreatedAt, Reputation 
                FROM Users 
                ORDER BY CreatedAt DESC";

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                users.Add(MapUser(reader));
            }

            return users;
        }

        public async Task UpdateReputationAsync(int userId, int reputationChange)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Users 
                SET Reputation = Reputation + @reputationChange 
                WHERE Id = @userId";

            command.Parameters.Add(new SqliteParameter("@userId", userId));
            command.Parameters.Add(new SqliteParameter("@reputationChange", reputationChange));

            await command.ExecuteNonQueryAsync();
        }

        private static User MapUser(SqliteDataReader reader)
        {
            return new User
            {
                Id = reader.GetInt32("Id"),
                Name = reader.GetString("Name"),
                Username = reader.GetString("Username"),
                Email = reader.GetString("Email"),
                PasswordHash = reader.GetString("PasswordHash"),
                CreatedAt = reader.GetDateTime("CreatedAt"),
                Reputation = reader.GetInt32("Reputation")
            };
        }
    }
}
