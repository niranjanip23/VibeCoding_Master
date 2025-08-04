# Database vs API Comparison Script
# This script compares what's in the database vs what the API returns

Write-Host "=== Database vs API Comparison ===" -ForegroundColor Green

$baseUrl = "http://localhost:5031/api"
$dbPath = "c:\Users\2317697\OneDrive - Cognizant\Desktop\CC\VibeCoding_Master-1\QueryHub-Backend\queryhub.db"

# Step 1: Get questions from API
Write-Host "`n1. Getting questions from API..." -ForegroundColor Yellow
try {
    $apiQuestions = Invoke-RestMethod -Uri "$baseUrl/questions" -Method GET
    Write-Host "   API returned $($apiQuestions.Count) questions"
    
    Write-Host "   Latest 5 questions from API:"
    $latestApi = $apiQuestions | Sort-Object id -Descending | Select-Object -First 5
    foreach ($q in $latestApi) {
        Write-Host "     ID: $($q.id) - $($q.title)" -ForegroundColor Cyan
    }
} catch {
    Write-Host "   Error getting API questions: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 2: Create a temporary C# console app to query the database directly
Write-Host "`n2. Creating database query tool..." -ForegroundColor Yellow

$dbQueryScript = @'
using System;
using System.Data.SQLite;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        string connectionString = "Data Source=queryhub.db";
        var questions = new List<(int Id, string Title)>();
        
        try
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                
                string query = @"
                    SELECT Id, Title, CreatedAt 
                    FROM Questions 
                    ORDER BY Id DESC 
                    LIMIT 10";
                
                using (var command = new SQLiteCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        Console.WriteLine($"Database connection successful!");
                        Console.WriteLine($"Questions found in database:");
                        
                        int count = 0;
                        while (reader.Read())
                        {
                            int id = reader.GetInt32("Id");
                            string title = reader.GetString("Title");
                            string createdAt = reader.GetString("CreatedAt");
                            Console.WriteLine($"  ID: {id} - {title} (Created: {createdAt})");
                            count++;
                        }
                        
                        if (count == 0)
                        {
                            Console.WriteLine("  No questions found in database!");
                        }
                        else
                        {
                            Console.WriteLine($"Total questions found: {count}");
                        }
                    }
                }
                
                // Also check total count
                using (var countCommand = new SQLiteCommand("SELECT COUNT(*) FROM Questions", connection))
                {
                    var totalCount = Convert.ToInt32(countCommand.ExecuteScalar());
                    Console.WriteLine($"Total question count in database: {totalCount}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
'@

# Save the C# script
$scriptPath = "c:\Users\2317697\OneDrive - Cognizant\Desktop\CC\VibeCoding_Master-1\QueryHub-Backend\DatabaseQuery.cs"
$dbQueryScript | Out-File -FilePath $scriptPath -Encoding UTF8

Write-Host "   Created database query script at: $scriptPath"

# Step 3: Compile and run the database query
Write-Host "`n3. Querying database directly..." -ForegroundColor Yellow
try {
    $currentDir = Get-Location
    Set-Location "c:\Users\2317697\OneDrive - Cognizant\Desktop\CC\VibeCoding_Master-1\QueryHub-Backend"
    
    # Try to compile and run the C# script
    Write-Host "   Compiling database query tool..."
    
    # Check if we can use dotnet script or need to create a project
    if (Get-Command "dotnet" -ErrorAction SilentlyContinue) {
        Write-Host "   Using dotnet to create a temporary console app..."
        
        # Create a temporary directory
        $tempDir = "temp_db_query"
        if (Test-Path $tempDir) {
            Remove-Item $tempDir -Recurse -Force
        }
        New-Item -ItemType Directory -Path $tempDir | Out-Null
        
        Set-Location $tempDir
        
        # Create a new console project
        & dotnet new console -n DbQuery --force | Out-Null
        Set-Location "DbQuery"
        
        # Add SQLite package
        & dotnet add package System.Data.SQLite.Core | Out-Null
        
        # Copy our database file to the project directory
        Copy-Item "../../../queryhub.db" "queryhub.db"
        
        # Replace Program.cs with our script
        $dbQueryScript | Out-File -FilePath "Program.cs" -Encoding UTF8
        
        # Run the query
        Write-Host "   Executing database query..."
        $dbResult = & dotnet run 2>&1
        Write-Host $dbResult
        
        # Cleanup
        Set-Location "../../.."
        Remove-Item $tempDir -Recurse -Force
    } else {
        Write-Host "   .NET CLI not available. Trying alternative method..." -ForegroundColor Yellow
        # Alternative: Try using PowerShell with System.Data.SQLite if available
        Write-Host "   Attempting to use PowerShell SQLite module..."
        
        try {
            # Try loading SQLite assembly
            [System.Reflection.Assembly]::LoadWithPartialName("System.Data.SQLite") | Out-Null
            
            $connectionString = "Data Source=$dbPath"
            $connection = New-Object System.Data.SQLite.SQLiteConnection($connectionString)
            $connection.Open()
            
            $command = $connection.CreateCommand()
            $command.CommandText = "SELECT Id, Title, CreatedAt FROM Questions ORDER BY Id DESC LIMIT 10"
            $reader = $command.ExecuteReader()
            
            Write-Host "   Questions found in database:"
            $dbCount = 0
            while ($reader.Read()) {
                $id = $reader["Id"]
                $title = $reader["Title"]
                $createdAt = $reader["CreatedAt"]
                Write-Host "     ID: $id - $title (Created: $createdAt)" -ForegroundColor Cyan
                $dbCount++
            }
            
            $reader.Close()
            
            # Get total count
            $countCommand = $connection.CreateCommand()
            $countCommand.CommandText = "SELECT COUNT(*) FROM Questions"
            $totalCount = $countCommand.ExecuteScalar()
            
            Write-Host "   Total questions in database: $totalCount"
            
            $connection.Close()
            
            if ($dbCount -eq 0) {
                Write-Host "   ⚠ No questions found in database!" -ForegroundColor Yellow
            }
            
        } catch {
            Write-Host "   Could not load SQLite assembly: $($_.Exception.Message)" -ForegroundColor Red
            Write-Host "   Database file exists: $(Test-Path $dbPath)"
            Write-Host "   Database file size: $((Get-Item $dbPath).Length) bytes"
        }
    }
    
    Set-Location $currentDir
} catch {
    Write-Host "   Error querying database: $($_.Exception.Message)" -ForegroundColor Red
    Set-Location $currentDir
}

Write-Host "`n=== Analysis ===" -ForegroundColor Green
Write-Host "If the API shows questions but the database query shows fewer or none,"
Write-Host "this could indicate:"
Write-Host "1. The application is using an in-memory cache"
Write-Host "2. The questions are stored in a different database file"
Write-Host "3. The application is reading from a different location"
Write-Host "4. There's a database transaction that hasn't been committed"
