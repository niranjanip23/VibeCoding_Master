using Microsoft.Data.Sqlite;
using QueryHub_Backend.Interfaces;
using QueryHub_Backend.Models;
using System.Data;

namespace QueryHub_Backend.Repositories
{
    public class TagRepository : ITagRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public TagRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<Tag?> GetByIdAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, Name, Description, CreatedAt 
                FROM Tags 
                WHERE Id = @id";
            command.Parameters.Add(new SqliteParameter("@id", id));

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapTag(reader);
            }

            return null;
        }

        public async Task<Tag?> GetByNameAsync(string name)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, Name, Description, CreatedAt 
                FROM Tags 
                WHERE Name = @name";
            command.Parameters.Add(new SqliteParameter("@name", name));

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapTag(reader);
            }

            return null;
        }

        public async Task<IEnumerable<Tag>> GetAllAsync()
        {
            var tags = new List<Tag>();

            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, Name, Description, CreatedAt 
                FROM Tags 
                ORDER BY Name";

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                tags.Add(MapTag(reader));
            }

            return tags;
        }

        public async Task<IEnumerable<Tag>> GetByQuestionIdAsync(int questionId)
        {
            var tags = new List<Tag>();

            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT t.Id, t.Name, t.Description, t.CreatedAt 
                FROM Tags t
                INNER JOIN QuestionTags qt ON t.Id = qt.TagId
                WHERE qt.QuestionId = @questionId 
                ORDER BY t.Name";
            command.Parameters.Add(new SqliteParameter("@questionId", questionId));

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                tags.Add(MapTag(reader));
            }

            return tags;
        }

        public async Task<IEnumerable<Tag>> SearchAsync(string searchTerm)
        {
            var tags = new List<Tag>();

            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, Name, Description, CreatedAt 
                FROM Tags 
                WHERE Name LIKE @searchTerm OR Description LIKE @searchTerm 
                ORDER BY Name";
            command.Parameters.Add(new SqliteParameter("@searchTerm", $"%{searchTerm}%"));

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                tags.Add(MapTag(reader));
            }

            return tags;
        }

        public async Task<Tag> CreateAsync(Tag tag)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Tags (Name, Description, CreatedAt)
                VALUES (@name, @description, @createdAt);
                SELECT last_insert_rowid();";

            command.Parameters.Add(new SqliteParameter("@name", tag.Name));
            command.Parameters.Add(new SqliteParameter("@description", tag.Description ?? (object)DBNull.Value));
            command.Parameters.Add(new SqliteParameter("@createdAt", tag.CreatedAt));

            var id = Convert.ToInt32(await command.ExecuteScalarAsync());
            tag.Id = id;

            return tag;
        }

        public async Task<Tag> UpdateAsync(Tag tag)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Tags 
                SET Name = @name, Description = @description 
                WHERE Id = @id";

            command.Parameters.Add(new SqliteParameter("@id", tag.Id));
            command.Parameters.Add(new SqliteParameter("@name", tag.Name));
            command.Parameters.Add(new SqliteParameter("@description", tag.Description ?? (object)DBNull.Value));

            await command.ExecuteNonQueryAsync();
            return tag;
        }

        public async Task DeleteAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Tags WHERE Id = @id";
            command.Parameters.Add(new SqliteParameter("@id", id));

            await command.ExecuteNonQueryAsync();
        }

        public async Task AddTagToQuestionAsync(int questionId, int tagId)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT OR IGNORE INTO QuestionTags (QuestionId, TagId) 
                VALUES (@questionId, @tagId)";
            command.Parameters.Add(new SqliteParameter("@questionId", questionId));
            command.Parameters.Add(new SqliteParameter("@tagId", tagId));

            await command.ExecuteNonQueryAsync();
        }

        public async Task RemoveTagFromQuestionAsync(int questionId, int tagId)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                DELETE FROM QuestionTags 
                WHERE QuestionId = @questionId AND TagId = @tagId";
            command.Parameters.Add(new SqliteParameter("@questionId", questionId));
            command.Parameters.Add(new SqliteParameter("@tagId", tagId));

            await command.ExecuteNonQueryAsync();
        }

        public async Task<int> GetQuestionCountByTagAsync(int tagId)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT COUNT(*) 
                FROM QuestionTags 
                WHERE TagId = @tagId";
            command.Parameters.Add(new SqliteParameter("@tagId", tagId));

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        private static Tag MapTag(SqliteDataReader reader)
        {
            return new Tag
            {
                Id = reader.GetInt32("Id"),
                Name = reader.GetString("Name"),
                Description = reader.IsDBNull("Description") ? string.Empty : reader.GetString("Description"),
                CreatedAt = reader.GetDateTime("CreatedAt")
            };
        }
    }
}
