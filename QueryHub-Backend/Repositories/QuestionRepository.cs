using Microsoft.Data.Sqlite;
using QueryHub_Backend.Interfaces;
using QueryHub_Backend.Models;
using System.Data;

namespace QueryHub_Backend.Repositories
{
    public class QuestionRepository : IQuestionRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public QuestionRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<Question?> GetByIdAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, Title, Description, Body, UserId, ViewCount, VoteCount, AnswerCount, CreatedAt, UpdatedAt, IsActive 
                FROM Questions 
                WHERE Id = @id";
            command.Parameters.Add(new SqliteParameter("@id", id));

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapQuestion(reader);
            }

            return null;
        }

        public async Task<IEnumerable<Question>> GetAllAsync()
        {
            var questions = new List<Question>();

            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, Title, Description, Body, UserId, ViewCount, VoteCount, AnswerCount, CreatedAt, UpdatedAt, IsActive 
                FROM Questions 
                ORDER BY CreatedAt DESC";

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                questions.Add(MapQuestion(reader));
            }

            return questions;
        }

        public async Task<IEnumerable<Question>> GetByUserIdAsync(int userId)
        {
            var questions = new List<Question>();

            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, Title, Description, Body, UserId, ViewCount, VoteCount, AnswerCount, CreatedAt, UpdatedAt, IsActive 
                FROM Questions 
                WHERE UserId = @userId 
                ORDER BY CreatedAt DESC";
            command.Parameters.Add(new SqliteParameter("@userId", userId));

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                questions.Add(MapQuestion(reader));
            }

            return questions;
        }

        public async Task<IEnumerable<Question>> SearchAsync(string searchTerm)
        {
            var questions = new List<Question>();

            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();
            
            // Parse search patterns
            var (sql, parameters) = BuildAdvancedSearchQuery(searchTerm);
            
            command.CommandText = sql;
            foreach (var param in parameters)
            {
                command.Parameters.Add(param);
            }

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                questions.Add(MapQuestion(reader));
            }

            return questions;
        }

        private (string sql, List<SqliteParameter> parameters) BuildAdvancedSearchQuery(string searchTerm)
        {
            var parameters = new List<SqliteParameter>();
            var conditions = new List<string>();
            var orderByConditions = new List<string>();
            
            // Base query
            var baseQuery = @"
                SELECT DISTINCT q.Id, q.Title, q.Description, q.Body, q.UserId, q.ViewCount, q.VoteCount, q.AnswerCount, q.CreatedAt, q.UpdatedAt, q.IsActive 
                FROM Questions q
                LEFT JOIN QuestionTags qt ON q.Id = qt.QuestionId
                LEFT JOIN Tags t ON qt.TagId = t.Id
                LEFT JOIN Users u ON q.UserId = u.Id";

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return ($"{baseQuery} ORDER BY q.VoteCount DESC, q.CreatedAt DESC", parameters);
            }

            var searchTermLower = searchTerm.ToLower().Trim();
            var priorityLevel = 1;

            // Pattern 1: [tag] - Search within a tag
            if (searchTermLower.StartsWith("[") && searchTermLower.EndsWith("]"))
            {
                var tagName = searchTermLower.Substring(1, searchTermLower.Length - 2);
                conditions.Add("LOWER(t.Name) LIKE @tagSearch");
                parameters.Add(new SqliteParameter("@tagSearch", $"%{tagName}%"));
                orderByConditions.Add($"CASE WHEN LOWER(t.Name) = @exactTag THEN {priorityLevel++} ELSE {priorityLevel + 10} END");
                parameters.Add(new SqliteParameter("@exactTag", tagName));
            }
            // Pattern 2: @username - Search by author
            else if (searchTermLower.StartsWith("@"))
            {
                var username = searchTermLower.Substring(1);
                conditions.Add("LOWER(u.Username) LIKE @authorSearch");
                parameters.Add(new SqliteParameter("@authorSearch", $"%{username}%"));
                orderByConditions.Add($"CASE WHEN LOWER(u.Username) = @exactAuthor THEN {priorityLevel++} ELSE {priorityLevel + 10} END");
                parameters.Add(new SqliteParameter("@exactAuthor", username));
            }
            // Pattern 3: collective:"Name" - Search collective content (placeholder for future feature)
            else if (searchTermLower.StartsWith("collective:\"") && searchTermLower.EndsWith("\""))
            {
                var collectiveName = searchTermLower.Substring(12, searchTermLower.Length - 13);
                // For now, search in question body for collective content
                conditions.Add("LOWER(q.Body) LIKE @collectiveSearch");
                parameters.Add(new SqliteParameter("@collectiveSearch", $"%{collectiveName}%"));
                orderByConditions.Add($"CASE WHEN LOWER(q.Body) LIKE @exactCollectiveSearch THEN {priorityLevel++} ELSE {priorityLevel + 10} END");
                parameters.Add(new SqliteParameter("@exactCollectiveSearch", $"%{collectiveName}%"));
            }
            // Pattern 4: Regular keyword search (title, body, description, tags)
            else
            {
                var regularSearch = new List<string>
                {
                    "LOWER(q.Title) LIKE @searchTerm",
                    "LOWER(q.Body) LIKE @searchTerm", 
                    "LOWER(q.Description) LIKE @searchTerm",
                    "LOWER(t.Name) LIKE @searchTerm"
                };
                
                conditions.Add($"({string.Join(" OR ", regularSearch)})");
                parameters.Add(new SqliteParameter("@searchTerm", $"%{searchTermLower}%"));
                
                // Add priority ordering for regular search
                orderByConditions.Add($"CASE WHEN LOWER(q.Title) = @exactSearchTerm THEN {priorityLevel++} ELSE 999 END");
                orderByConditions.Add($"CASE WHEN LOWER(t.Name) = @exactSearchTerm THEN {priorityLevel++} ELSE 999 END");
                orderByConditions.Add($"CASE WHEN LOWER(q.Title) LIKE @exactSearchTerm THEN {priorityLevel++} ELSE 999 END");
                orderByConditions.Add($"CASE WHEN LOWER(t.Name) LIKE @exactSearchTerm THEN {priorityLevel++} ELSE 999 END");
                
                parameters.Add(new SqliteParameter("@exactSearchTerm", searchTermLower));
            }

            var whereClause = conditions.Count > 0 ? $"WHERE {string.Join(" OR ", conditions)}" : "";
            var orderByClause = orderByConditions.Count > 0 
                ? $"ORDER BY {string.Join(", ", orderByConditions)}, q.VoteCount DESC, q.CreatedAt DESC"
                : "ORDER BY q.VoteCount DESC, q.CreatedAt DESC";

            var finalQuery = $"{baseQuery} {whereClause} {orderByClause}";
            
            return (finalQuery, parameters);
        }

        public async Task<IEnumerable<Question>> GetByTagAsync(string tagName)
        {
            var questions = new List<Question>();

            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT DISTINCT q.Id, q.Title, q.Description, q.Body, q.UserId, q.ViewCount, q.VoteCount, q.AnswerCount, q.CreatedAt, q.UpdatedAt, q.IsActive 
                FROM Questions q
                INNER JOIN QuestionTags qt ON q.Id = qt.QuestionId
                INNER JOIN Tags t ON qt.TagId = t.Id
                WHERE t.Name = @tagName 
                ORDER BY q.VoteCount DESC, q.CreatedAt DESC";
            command.Parameters.Add(new SqliteParameter("@tagName", tagName));

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                questions.Add(MapQuestion(reader));
            }

            return questions;
        }

        public async Task<Question> CreateAsync(Question question)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Questions (Title, Description, Body, UserId, CreatedAt, UpdatedAt, Views, VoteCount)
                VALUES (@title, @description, @body, @userId, @createdAt, @updatedAt, @views, @voteCount);
                SELECT last_insert_rowid();";

            command.Parameters.Add(new SqliteParameter("@title", question.Title));
            command.Parameters.Add(new SqliteParameter("@description", question.Description));
            command.Parameters.Add(new SqliteParameter("@body", question.Body));
            command.Parameters.Add(new SqliteParameter("@userId", question.UserId));
            command.Parameters.Add(new SqliteParameter("@createdAt", question.CreatedAt));
            command.Parameters.Add(new SqliteParameter("@updatedAt", question.UpdatedAt));
            command.Parameters.Add(new SqliteParameter("@views", question.Views));
            command.Parameters.Add(new SqliteParameter("@voteCount", question.VoteCount));

            var id = Convert.ToInt32(await command.ExecuteScalarAsync());
            question.Id = id;

            return question;
        }

        public async Task<Question> UpdateAsync(Question question)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Questions 
                SET Title = @title, Description = @description, Body = @body, UpdatedAt = @updatedAt, Views = @views, VoteCount = @voteCount
                WHERE Id = @id";

            command.Parameters.Add(new SqliteParameter("@id", question.Id));
            command.Parameters.Add(new SqliteParameter("@title", question.Title));
            command.Parameters.Add(new SqliteParameter("@description", question.Description));
            command.Parameters.Add(new SqliteParameter("@body", question.Body));
            command.Parameters.Add(new SqliteParameter("@updatedAt", question.UpdatedAt));
            command.Parameters.Add(new SqliteParameter("@views", question.Views));
            command.Parameters.Add(new SqliteParameter("@voteCount", question.VoteCount));

            await command.ExecuteNonQueryAsync();
            return question;
        }

        public async Task DeleteAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Questions WHERE Id = @id";
            command.Parameters.Add(new SqliteParameter("@id", id));

            await command.ExecuteNonQueryAsync();
        }

        public async Task IncrementViewsAsync(int questionId)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Questions 
                SET ViewCount = ViewCount + 1 
                WHERE Id = @questionId";
            command.Parameters.Add(new SqliteParameter("@questionId", questionId));

            await command.ExecuteNonQueryAsync();
        }

        public async Task UpdateVotesAsync(int questionId, int voteChange)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Questions 
                SET VoteCount = VoteCount + @voteChange 
                WHERE Id = @questionId";
            command.Parameters.Add(new SqliteParameter("@questionId", questionId));
            command.Parameters.Add(new SqliteParameter("@voteChange", voteChange));

            await command.ExecuteNonQueryAsync();
        }

        private static Question MapQuestion(SqliteDataReader reader)
        {
            return new Question
            {
                Id = reader.GetInt32("Id"),
                Title = reader.GetString("Title"),
                Description = reader.GetString("Description"),
                Body = reader.GetString("Body"),
                UserId = reader.GetInt32("UserId"),
                ViewCount = reader.GetInt32("ViewCount"),
                Views = reader.GetInt32("ViewCount"), // Map to both for compatibility
                VoteCount = reader.GetInt32("VoteCount"),
                AnswerCount = reader.GetInt32("AnswerCount"),
                CreatedAt = reader.GetDateTime("CreatedAt"),
                UpdatedAt = reader.GetDateTime("UpdatedAt"),
                IsActive = reader.GetBoolean("IsActive")
            };
        }
    }
}
