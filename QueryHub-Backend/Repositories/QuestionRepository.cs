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
            command.CommandText = @"
                SELECT DISTINCT q.Id, q.Title, q.Description, q.Body, q.UserId, q.ViewCount, q.VoteCount, q.AnswerCount, q.CreatedAt, q.UpdatedAt, q.IsActive 
                FROM Questions q
                LEFT JOIN QuestionTags qt ON q.Id = qt.QuestionId
                LEFT JOIN Tags t ON qt.TagId = t.Id
                WHERE q.Title LIKE @searchTerm 
                   OR q.Body LIKE @searchTerm 
                   OR q.Description LIKE @searchTerm 
                   OR t.Name LIKE @searchTerm
                   OR t.Name LIKE @exactSearchTerm
                ORDER BY 
                    CASE 
                        WHEN q.Title LIKE @exactSearchTerm THEN 1
                        WHEN t.Name LIKE @exactSearchTerm THEN 2
                        WHEN q.Title LIKE @searchTerm THEN 3
                        WHEN t.Name LIKE @searchTerm THEN 4
                        ELSE 5
                    END,
                    q.VoteCount DESC, 
                    q.CreatedAt DESC";
            
            command.Parameters.Add(new SqliteParameter("@searchTerm", $"%{searchTerm}%"));
            command.Parameters.Add(new SqliteParameter("@exactSearchTerm", searchTerm));

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                questions.Add(MapQuestion(reader));
            }

            return questions;
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
