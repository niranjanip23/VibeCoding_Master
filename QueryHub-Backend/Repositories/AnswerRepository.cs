using Microsoft.Data.Sqlite;
using QueryHub_Backend.Interfaces;
using QueryHub_Backend.Models;
using System.Data;

namespace QueryHub_Backend.Repositories
{
    public class AnswerRepository : IAnswerRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public AnswerRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<Answer?> GetByIdAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, Content, Body, QuestionId, UserId, CreatedAt, UpdatedAt, VoteCount, IsAccepted, IsActive 
                FROM Answers 
                WHERE Id = @id AND IsActive = 1";
            command.Parameters.Add(new SqliteParameter("@id", id));

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapAnswer(reader);
            }

            return null;
        }

        public async Task<IEnumerable<Answer>> GetByQuestionIdAsync(int questionId)
        {
            var answers = new List<Answer>();

            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, Content, Body, QuestionId, UserId, CreatedAt, UpdatedAt, VoteCount, IsAccepted, IsActive 
                FROM Answers 
                WHERE QuestionId = @questionId AND IsActive = 1 
                ORDER BY IsAccepted DESC, VoteCount DESC, CreatedAt ASC";
            command.Parameters.Add(new SqliteParameter("@questionId", questionId));

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                answers.Add(MapAnswer(reader));
            }

            return answers;
        }

        public async Task<IEnumerable<Answer>> GetByUserIdAsync(int userId)
        {
            var answers = new List<Answer>();

            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, Body, QuestionId, UserId, CreatedAt, UpdatedAt, VoteCount, IsAccepted 
                FROM Answers 
                WHERE UserId = @userId 
                ORDER BY CreatedAt DESC";
            command.Parameters.Add(new SqliteParameter("@userId", userId));

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                answers.Add(MapAnswer(reader));
            }

            return answers;
        }

        public async Task<Answer> CreateAsync(Answer answer)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Answers (Content, Body, QuestionId, UserId, CreatedAt, UpdatedAt, VoteCount, IsAccepted, IsActive)
                VALUES (@content, @body, @questionId, @userId, @createdAt, @updatedAt, @voteCount, @isAccepted, @isActive);
                SELECT last_insert_rowid();";

            command.Parameters.Add(new SqliteParameter("@content", answer.Content));
            command.Parameters.Add(new SqliteParameter("@body", answer.Body));
            command.Parameters.Add(new SqliteParameter("@questionId", answer.QuestionId));
            command.Parameters.Add(new SqliteParameter("@userId", answer.UserId));
            command.Parameters.Add(new SqliteParameter("@createdAt", answer.CreatedAt));
            command.Parameters.Add(new SqliteParameter("@updatedAt", answer.UpdatedAt));
            command.Parameters.Add(new SqliteParameter("@voteCount", answer.VoteCount));
            command.Parameters.Add(new SqliteParameter("@isAccepted", answer.IsAccepted));
            command.Parameters.Add(new SqliteParameter("@isActive", answer.IsActive));

            var id = Convert.ToInt32(await command.ExecuteScalarAsync());
            answer.Id = id;

            return answer;
        }

        public async Task<Answer> UpdateAsync(Answer answer)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Answers 
                SET Content = @content, Body = @body, UpdatedAt = @updatedAt, VoteCount = @voteCount, IsAccepted = @isAccepted
                WHERE Id = @id";

            command.Parameters.Add(new SqliteParameter("@id", answer.Id));
            command.Parameters.Add(new SqliteParameter("@content", answer.Content));
            command.Parameters.Add(new SqliteParameter("@body", answer.Body));
            command.Parameters.Add(new SqliteParameter("@updatedAt", answer.UpdatedAt));
            command.Parameters.Add(new SqliteParameter("@voteCount", answer.VoteCount));
            command.Parameters.Add(new SqliteParameter("@isAccepted", answer.IsAccepted));

            await command.ExecuteNonQueryAsync();
            return answer;
        }

        public async Task DeleteAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Answers WHERE Id = @id";
            command.Parameters.Add(new SqliteParameter("@id", id));

            await command.ExecuteNonQueryAsync();
        }

        public async Task UpdateVotesAsync(int answerId, int voteChange)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Answers 
                SET Votes = Votes + @voteChange 
                WHERE Id = @answerId";
            command.Parameters.Add(new SqliteParameter("@answerId", answerId));
            command.Parameters.Add(new SqliteParameter("@voteChange", voteChange));

            await command.ExecuteNonQueryAsync();
        }

        public async Task MarkAsAcceptedAsync(int answerId, int questionId)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // First, unmark any previously accepted answer for this question
                var unmarkCommand = connection.CreateCommand();
                unmarkCommand.Transaction = transaction;
                unmarkCommand.CommandText = @"
                    UPDATE Answers 
                    SET IsAccepted = 0 
                    WHERE QuestionId = @questionId AND IsAccepted = 1";
                unmarkCommand.Parameters.Add(new SqliteParameter("@questionId", questionId));
                await unmarkCommand.ExecuteNonQueryAsync();

                // Then mark the specified answer as accepted
                var markCommand = connection.CreateCommand();
                markCommand.Transaction = transaction;
                markCommand.CommandText = @"
                    UPDATE Answers 
                    SET IsAccepted = 1 
                    WHERE Id = @answerId";
                markCommand.Parameters.Add(new SqliteParameter("@answerId", answerId));
                await markCommand.ExecuteNonQueryAsync();

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        private static Answer MapAnswer(SqliteDataReader reader)
        {
            return new Answer
            {
                Id = reader.GetInt32("Id"),
                Content = reader.GetString("Content"),
                Body = reader.GetString("Body"),
                QuestionId = reader.GetInt32("QuestionId"),
                UserId = reader.GetInt32("UserId"),
                CreatedAt = reader.GetDateTime("CreatedAt"),
                UpdatedAt = reader.GetDateTime("UpdatedAt"),
                VoteCount = reader.GetInt32("VoteCount"),
                IsAccepted = reader.GetBoolean("IsAccepted"),
                IsActive = reader.GetBoolean("IsActive")
            };
        }
    }
}
