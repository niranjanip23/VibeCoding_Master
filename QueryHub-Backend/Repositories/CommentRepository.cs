using Microsoft.Data.Sqlite;
using QueryHub_Backend.Interfaces;
using QueryHub_Backend.Models;
using System.Data;

namespace QueryHub_Backend.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public CommentRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<Comment?> GetByIdAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, Body, QuestionId, AnswerId, UserId, CreatedAt 
                FROM Comments 
                WHERE Id = @id";
            command.Parameters.Add(new SqliteParameter("@id", id));

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapComment(reader);
            }

            return null;
        }

        public async Task<IEnumerable<Comment>> GetByQuestionIdAsync(int questionId)
        {
            var comments = new List<Comment>();

            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, Body, QuestionId, AnswerId, UserId, CreatedAt 
                FROM Comments 
                WHERE QuestionId = @questionId AND AnswerId IS NULL 
                ORDER BY CreatedAt ASC";
            command.Parameters.Add(new SqliteParameter("@questionId", questionId));

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                comments.Add(MapComment(reader));
            }

            return comments;
        }

        public async Task<IEnumerable<Comment>> GetByAnswerIdAsync(int answerId)
        {
            var comments = new List<Comment>();

            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, Body, QuestionId, AnswerId, UserId, CreatedAt 
                FROM Comments 
                WHERE AnswerId = @answerId 
                ORDER BY CreatedAt ASC";
            command.Parameters.Add(new SqliteParameter("@answerId", answerId));

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                comments.Add(MapComment(reader));
            }

            return comments;
        }

        public async Task<IEnumerable<Comment>> GetByUserIdAsync(int userId)
        {
            var comments = new List<Comment>();

            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, Body, QuestionId, AnswerId, UserId, CreatedAt 
                FROM Comments 
                WHERE UserId = @userId 
                ORDER BY CreatedAt DESC";
            command.Parameters.Add(new SqliteParameter("@userId", userId));

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                comments.Add(MapComment(reader));
            }

            return comments;
        }

        public async Task<Comment> CreateAsync(Comment comment)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Comments (Body, QuestionId, AnswerId, UserId, CreatedAt)
                VALUES (@body, @questionId, @answerId, @userId, @createdAt);
                SELECT last_insert_rowid();";

            command.Parameters.Add(new SqliteParameter("@body", comment.Body));
            command.Parameters.Add(new SqliteParameter("@questionId", comment.QuestionId));
            command.Parameters.Add(new SqliteParameter("@answerId", (object?)comment.AnswerId ?? DBNull.Value));
            command.Parameters.Add(new SqliteParameter("@userId", comment.UserId));
            command.Parameters.Add(new SqliteParameter("@createdAt", comment.CreatedAt));

            var id = Convert.ToInt32(await command.ExecuteScalarAsync());
            comment.Id = id;

            return comment;
        }

        public async Task<Comment> UpdateAsync(Comment comment)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Comments 
                SET Body = @body 
                WHERE Id = @id";

            command.Parameters.Add(new SqliteParameter("@id", comment.Id));
            command.Parameters.Add(new SqliteParameter("@body", comment.Body));

            await command.ExecuteNonQueryAsync();
            return comment;
        }

        public async Task DeleteAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Comments WHERE Id = @id";
            command.Parameters.Add(new SqliteParameter("@id", id));

            await command.ExecuteNonQueryAsync();
        }

        private static Comment MapComment(SqliteDataReader reader)
        {
            return new Comment
            {
                Id = reader.GetInt32("Id"),
                Body = reader.GetString("Body"),
                QuestionId = reader.GetInt32("QuestionId"),
                AnswerId = reader.IsDBNull("AnswerId") ? null : reader.GetInt32("AnswerId"),
                UserId = reader.GetInt32("UserId"),
                CreatedAt = reader.GetDateTime("CreatedAt")
            };
        }
    }
}
