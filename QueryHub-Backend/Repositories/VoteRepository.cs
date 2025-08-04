using Microsoft.Data.Sqlite;
using QueryHub_Backend.Interfaces;
using QueryHub_Backend.Models;
using System.Data;

namespace QueryHub_Backend.Repositories
{
    public class VoteRepository : IVoteRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public VoteRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<Vote?> GetByUserAndQuestionAsync(int userId, int questionId)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, UserId, QuestionId, AnswerId, VoteType, CreatedAt 
                FROM Votes 
                WHERE UserId = @userId AND QuestionId = @questionId AND AnswerId IS NULL";
            command.Parameters.Add(new SqliteParameter("@userId", userId));
            command.Parameters.Add(new SqliteParameter("@questionId", questionId));

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapVote(reader);
            }

            return null;
        }

        public async Task<Vote?> GetByUserAndAnswerAsync(int userId, int answerId)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, UserId, QuestionId, AnswerId, VoteType, CreatedAt 
                FROM Votes 
                WHERE UserId = @userId AND AnswerId = @answerId";
            command.Parameters.Add(new SqliteParameter("@userId", userId));
            command.Parameters.Add(new SqliteParameter("@answerId", answerId));

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapVote(reader);
            }

            return null;
        }

        public async Task<IEnumerable<Vote>> GetByUserIdAsync(int userId)
        {
            var votes = new List<Vote>();

            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, UserId, QuestionId, AnswerId, VoteType, CreatedAt 
                FROM Votes 
                WHERE UserId = @userId 
                ORDER BY CreatedAt DESC";
            command.Parameters.Add(new SqliteParameter("@userId", userId));

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                votes.Add(MapVote(reader));
            }

            return votes;
        }

        public async Task<Vote> CreateAsync(Vote vote)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Votes (UserId, QuestionId, AnswerId, VoteType, CreatedAt)
                VALUES (@userId, @questionId, @answerId, @voteType, @createdAt);
                SELECT last_insert_rowid();";

            command.Parameters.Add(new SqliteParameter("@userId", vote.UserId));
            command.Parameters.Add(new SqliteParameter("@questionId", vote.QuestionId));
            command.Parameters.Add(new SqliteParameter("@answerId", (object?)vote.AnswerId ?? DBNull.Value));
            command.Parameters.Add(new SqliteParameter("@voteType", (int)vote.VoteType));
            command.Parameters.Add(new SqliteParameter("@createdAt", vote.CreatedAt));

            var id = Convert.ToInt32(await command.ExecuteScalarAsync());
            vote.Id = id;

            return vote;
        }

        public async Task<Vote> UpdateAsync(Vote vote)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Votes 
                SET VoteType = @voteType 
                WHERE Id = @id";

            command.Parameters.Add(new SqliteParameter("@id", vote.Id));
            command.Parameters.Add(new SqliteParameter("@voteType", (int)vote.VoteType));

            await command.ExecuteNonQueryAsync();
            return vote;
        }

        public async Task DeleteAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Votes WHERE Id = @id";
            command.Parameters.Add(new SqliteParameter("@id", id));

            await command.ExecuteNonQueryAsync();
        }

        public async Task<int> GetQuestionVoteCountAsync(int questionId)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT COALESCE(SUM(CASE WHEN VoteType = 1 THEN 1 WHEN VoteType = 2 THEN -1 ELSE 0 END), 0) as VoteCount
                FROM Votes 
                WHERE QuestionId = @questionId AND AnswerId IS NULL";
            command.Parameters.Add(new SqliteParameter("@questionId", questionId));

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<int> GetAnswerVoteCountAsync(int answerId)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT COALESCE(SUM(CASE WHEN VoteType = 1 THEN 1 WHEN VoteType = 2 THEN -1 ELSE 0 END), 0) as VoteCount
                FROM Votes 
                WHERE AnswerId = @answerId";
            command.Parameters.Add(new SqliteParameter("@answerId", answerId));

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        private static Vote MapVote(SqliteDataReader reader)
        {
            return new Vote
            {
                Id = reader.GetInt32("Id"),
                UserId = reader.GetInt32("UserId"),
                QuestionId = reader.GetInt32("QuestionId"),
                AnswerId = reader.IsDBNull("AnswerId") ? null : reader.GetInt32("AnswerId"),
                VoteType = (VoteType)reader.GetInt32("VoteType"),
                CreatedAt = reader.GetDateTime("CreatedAt")
            };
        }
    }
}
