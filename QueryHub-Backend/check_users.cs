using System.Data.SQLite;

string connectionString = "Data Source=queryhub.db";
using var connection = new SQLiteConnection(connectionString);
connection.Open();

var command = new SQLiteCommand("SELECT id, username, email, name FROM Users ORDER BY id", connection);
using var reader = command.ExecuteReader();

Console.WriteLine("Users in database:");
Console.WriteLine("ID | Username | Email | Name");
Console.WriteLine("---|----------|--------|------");

while (reader.Read())
{
    Console.WriteLine($"{reader["id"]} | {reader["username"]} | {reader["email"]} | {reader["name"]}");
}
