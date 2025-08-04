using System;
using System.Data.SQLite;
using System.IO;

namespace DatabaseChecker
{
    class DatabaseReader
    {
        static void Main()
        {
            string dbPath = "queryhub.db";
            
            if (!File.Exists(dbPath))
            {
                Console.WriteLine("Database file not found: " + dbPath);
                return;
            }

            string connectionString = $"Data Source={dbPath};Version=3;";
            
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    
                    // Get recent questions
                    Console.WriteLine("=== RECENT QUESTIONS (Last 10) ===");
                    string query = @"
                        SELECT q.Id, q.Title, q.Body, q.UserId, q.CreatedAt, q.UpdatedAt,
                               u.Username as UserName
                        FROM Questions q
                        LEFT JOIN Users u ON q.UserId = u.Id
                        ORDER BY q.CreatedAt DESC 
                        LIMIT 10";
                    
                    using (var command = new SQLiteCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Console.WriteLine($"ID: {reader["Id"]}");
                            Console.WriteLine($"Title: {reader["Title"]}");
                            string body = reader["Body"]?.ToString() ?? "";
                            Console.WriteLine($"Body: {(body.Length > 100 ? body.Substring(0, 100) + "..." : body)}");
                            Console.WriteLine($"User: {reader["UserName"]} (ID: {reader["UserId"]})");
                            Console.WriteLine($"Created: {reader["CreatedAt"]}");
                            Console.WriteLine($"Updated: {reader["UpdatedAt"]}");
                            Console.WriteLine("---");
                        }
                    }
                    
                    // Get question count
                    Console.WriteLine("\n=== QUESTION STATISTICS ===");
                    string countQuery = "SELECT COUNT(*) as Total FROM Questions";
                    using (var command = new SQLiteCommand(countQuery, connection))
                    {
                        var total = command.ExecuteScalar();
                        Console.WriteLine($"Total Questions: {total}");
                    }
                    
                    // Get questions created today
                    string todayQuery = @"
                        SELECT COUNT(*) as TodayCount 
                        FROM Questions 
                        WHERE DATE(CreatedAt) = DATE('now')";
                    using (var command = new SQLiteCommand(todayQuery, connection))
                    {
                        var todayCount = command.ExecuteScalar();
                        Console.WriteLine($"Questions Created Today: {todayCount}");
                    }
                    
                    // Get tags for recent questions
                    Console.WriteLine("\n=== RECENT QUESTION TAGS ===");
                    string tagQuery = @"
                        SELECT q.Id, q.Title, GROUP_CONCAT(t.Name, ', ') as Tags
                        FROM Questions q
                        LEFT JOIN QuestionTags qt ON q.Id = qt.QuestionId
                        LEFT JOIN Tags t ON qt.TagId = t.Id
                        GROUP BY q.Id, q.Title
                        ORDER BY q.CreatedAt DESC
                        LIMIT 5";
                    
                    using (var command = new SQLiteCommand(tagQuery, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Console.WriteLine($"Q{reader["Id"]}: {reader["Title"]}");
                            Console.WriteLine($"   Tags: {reader["Tags"] ?? "No tags"}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
