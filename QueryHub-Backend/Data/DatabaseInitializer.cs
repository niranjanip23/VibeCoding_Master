using Microsoft.Data.Sqlite;
using QueryHub_Backend.Interfaces;

namespace QueryHub_Backend.Data
{
    public class DatabaseInitializer : IDatabaseInitializer
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<DatabaseInitializer> _logger;

        public DatabaseInitializer(IDbConnectionFactory connectionFactory, ILogger<DatabaseInitializer> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            
            await CreateTablesAsync(connection);
            _logger.LogInformation("Database tables created successfully");
        }

        private async Task CreateTablesAsync(SqliteConnection connection)
        {
            // Create Users table
            var createUsersTable = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Username TEXT NOT NULL UNIQUE,
                    Email TEXT NOT NULL UNIQUE,
                    PasswordHash TEXT NOT NULL,
                    Department TEXT,
                    Avatar TEXT,
                    Reputation INTEGER DEFAULT 0,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    IsActive BOOLEAN DEFAULT 1
                );";

            // Create Questions table
            var createQuestionsTable = @"
                CREATE TABLE IF NOT EXISTS Questions (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Title TEXT NOT NULL,
                    Description TEXT NOT NULL,
                    Body TEXT NOT NULL,
                    UserId INTEGER NOT NULL,
                    ViewCount INTEGER DEFAULT 0,
                    Views INTEGER DEFAULT 0,
                    VoteCount INTEGER DEFAULT 0,
                    AnswerCount INTEGER DEFAULT 0,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    IsActive BOOLEAN DEFAULT 1,
                    FOREIGN KEY (UserId) REFERENCES Users(Id)
                );";

            // Create Answers table
            var createAnswersTable = @"
                CREATE TABLE IF NOT EXISTS Answers (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Content TEXT NOT NULL,
                    Body TEXT NOT NULL,
                    QuestionId INTEGER NOT NULL,
                    UserId INTEGER NOT NULL,
                    VoteCount INTEGER DEFAULT 0,
                    IsAccepted BOOLEAN DEFAULT 0,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    IsActive BOOLEAN DEFAULT 1,
                    FOREIGN KEY (QuestionId) REFERENCES Questions(Id),
                    FOREIGN KEY (UserId) REFERENCES Users(Id)
                );";

            // Create Tags table
            var createTagsTable = @"
                CREATE TABLE IF NOT EXISTS Tags (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL UNIQUE,
                    Description TEXT,
                    UsageCount INTEGER DEFAULT 0,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                );";

            // Create QuestionTags table (many-to-many)
            var createQuestionTagsTable = @"
                CREATE TABLE IF NOT EXISTS QuestionTags (
                    QuestionId INTEGER NOT NULL,
                    TagId INTEGER NOT NULL,
                    PRIMARY KEY (QuestionId, TagId),
                    FOREIGN KEY (QuestionId) REFERENCES Questions(Id) ON DELETE CASCADE,
                    FOREIGN KEY (TagId) REFERENCES Tags(Id) ON DELETE CASCADE
                );";

            // Create Votes table
            var createVotesTable = @"
                CREATE TABLE IF NOT EXISTS Votes (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    UserId INTEGER NOT NULL,
                    TargetId INTEGER NOT NULL,
                    TargetType INTEGER NOT NULL, -- 1=Question, 2=Answer
                    VoteType INTEGER NOT NULL, -- 1=Upvote, -1=Downvote
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    UNIQUE(UserId, TargetId, TargetType),
                    FOREIGN KEY (UserId) REFERENCES Users(Id)
                );";

            // Create Comments table
            var createCommentsTable = @"
                CREATE TABLE IF NOT EXISTS Comments (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Content TEXT NOT NULL,
                    UserId INTEGER NOT NULL,
                    TargetId INTEGER NOT NULL,
                    TargetType INTEGER NOT NULL, -- 1=Question, 2=Answer
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    IsActive BOOLEAN DEFAULT 1,
                    FOREIGN KEY (UserId) REFERENCES Users(Id)
                );";

            // Execute all table creation commands
            var tables = new[]
            {
                createUsersTable,
                createQuestionsTable,
                createAnswersTable,
                createTagsTable,
                createQuestionTagsTable,
                createVotesTable,
                createCommentsTable
            };

            foreach (var table in tables)
            {
                using var command = new SqliteCommand(table, connection);
                await command.ExecuteNonQueryAsync();
            }

            // Create indexes for better performance
            await CreateIndexesAsync(connection);
        }

        private async Task CreateIndexesAsync(SqliteConnection connection)
        {
            var indexes = new[]
            {
                "CREATE INDEX IF NOT EXISTS IX_Users_Email ON Users(Email);",
                "CREATE INDEX IF NOT EXISTS IX_Questions_UserId ON Questions(UserId);",
                "CREATE INDEX IF NOT EXISTS IX_Questions_CreatedAt ON Questions(CreatedAt DESC);",
                "CREATE INDEX IF NOT EXISTS IX_Answers_QuestionId ON Answers(QuestionId);",
                "CREATE INDEX IF NOT EXISTS IX_Answers_UserId ON Answers(UserId);",
                "CREATE INDEX IF NOT EXISTS IX_Votes_UserId ON Votes(UserId);",
                "CREATE INDEX IF NOT EXISTS IX_Votes_Target ON Votes(TargetId, TargetType);",
                "CREATE INDEX IF NOT EXISTS IX_Comments_Target ON Comments(TargetId, TargetType);",
                "CREATE INDEX IF NOT EXISTS IX_Tags_Name ON Tags(Name);"
            };

            foreach (var index in indexes)
            {
                using var command = new SqliteCommand(index, connection);
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task SeedDataAsync()
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            
            // Check if data already exists
            var checkUser = "SELECT COUNT(*) FROM Users;";
            using var checkCommand = new SqliteCommand(checkUser, connection);
            var userCount = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());
            
            if (userCount > 0)
            {
                _logger.LogInformation("Data already seeded");
                return;
            }

            // Seed sample data
            await SeedUsersAsync(connection);
            await SeedTagsAsync(connection);
            await SeedQuestionsAsync(connection);
            
            _logger.LogInformation("Sample data seeded successfully");
        }

        private async Task SeedUsersAsync(SqliteConnection connection)
        {
            var users = new[]
            {
                ("John Doe", "john@queryhub.com", BCrypt.Net.BCrypt.HashPassword("password123"), "IT Department"),
                ("Jane Smith", "jane@queryhub.com", BCrypt.Net.BCrypt.HashPassword("password123"), "Development"),
                ("Bob Johnson", "bob@queryhub.com", BCrypt.Net.BCrypt.HashPassword("password123"), "QA Team")
            };

            foreach (var (name, email, passwordHash, department) in users)
            {
                var sql = @"
                    INSERT INTO Users (Name, Username, Email, PasswordHash, Department, Reputation) 
                    VALUES (@Name, @Username, @Email, @PasswordHash, @Department, @Reputation);";
                
                using var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("@Name", name);
                command.Parameters.AddWithValue("@Username", name.Replace(" ", "").ToLower()); // Generate username from name
                command.Parameters.AddWithValue("@Email", email);
                command.Parameters.AddWithValue("@PasswordHash", passwordHash);
                command.Parameters.AddWithValue("@Department", department);
                command.Parameters.AddWithValue("@Reputation", new Random().Next(100, 1000));
                
                await command.ExecuteNonQueryAsync();
            }
        }

        private async Task SeedTagsAsync(SqliteConnection connection)
        {
            var tags = new[]
            {
                ("C#", "Microsoft's programming language"),
                ("ASP.NET", "Web framework for .NET"),
                ("JavaScript", "Programming language for web development"),
                ("React", "JavaScript library for building user interfaces"),
                ("SQL", "Structured Query Language for databases"),
                ("HTML", "HyperText Markup Language"),
                ("CSS", "Cascading Style Sheets"),
                ("Python", "High-level programming language"),
                ("Node.js", "JavaScript runtime environment"),
                ("TypeScript", "Typed superset of JavaScript")
            };

            foreach (var (name, description) in tags)
            {
                var sql = "INSERT INTO Tags (Name, Description) VALUES (@Name, @Description);";
                using var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("@Name", name);
                command.Parameters.AddWithValue("@Description", description);
                await command.ExecuteNonQueryAsync();
            }
        }

        private async Task SeedQuestionsAsync(SqliteConnection connection)
        {
            var questions = new[]
            {
                ("How to implement dependency injection in ASP.NET Core?", 
                 "I'm new to ASP.NET Core and want to understand how to properly implement dependency injection. Can someone provide a comprehensive example?",
                 1, new[] { "C#", "ASP.NET" }),
                
                ("Best practices for React state management?", 
                 "What are the current best practices for managing state in React applications? Should I use Redux, Context API, or something else?",
                 2, new[] { "React", "JavaScript" }),
                
                ("SQL optimization techniques for large datasets?", 
                 "My queries are running slowly on large tables. What are some effective optimization techniques I can apply?",
                 3, new[] { "SQL" })
            };

            foreach (var (title, description, userId, tagNames) in questions)
            {
                // Insert question
                var insertQuestion = @"
                    INSERT INTO Questions (Title, Description, Body, UserId) 
                    VALUES (@Title, @Description, @Body, @UserId);
                    SELECT last_insert_rowid();";
                
                using var questionCommand = new SqliteCommand(insertQuestion, connection);
                questionCommand.Parameters.AddWithValue("@Title", title);
                questionCommand.Parameters.AddWithValue("@Description", description);
                questionCommand.Parameters.AddWithValue("@Body", description); // Using description as body for now
                questionCommand.Parameters.AddWithValue("@UserId", userId);
                
                var questionId = Convert.ToInt32(await questionCommand.ExecuteScalarAsync());

                // Link tags to question
                foreach (var tagName in tagNames)
                {
                    var getTagId = "SELECT Id FROM Tags WHERE Name = @TagName;";
                    using var tagCommand = new SqliteCommand(getTagId, connection);
                    tagCommand.Parameters.AddWithValue("@TagName", tagName);
                    var tagId = await tagCommand.ExecuteScalarAsync();

                    if (tagId != null)
                    {
                        var linkTag = "INSERT INTO QuestionTags (QuestionId, TagId) VALUES (@QuestionId, @TagId);";
                        using var linkCommand = new SqliteCommand(linkTag, connection);
                        linkCommand.Parameters.AddWithValue("@QuestionId", questionId);
                        linkCommand.Parameters.AddWithValue("@TagId", tagId);
                        await linkCommand.ExecuteNonQueryAsync();

                        // Update tag usage count
                        var updateUsage = "UPDATE Tags SET UsageCount = UsageCount + 1 WHERE Id = @TagId;";
                        using var updateCommand = new SqliteCommand(updateUsage, connection);
                        updateCommand.Parameters.AddWithValue("@TagId", tagId);
                        await updateCommand.ExecuteNonQueryAsync();
                    }
                }
            }
        }
    }
}
