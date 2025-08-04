# Direct Database Verification Script
# This script queries the SQLite database directly to verify persistence

Write-Host "=== Direct Database Verification ===" -ForegroundColor Green

$dbPath = "c:\Users\2317697\OneDrive - Cognizant\Desktop\CC\VibeCoding_Master-1\QueryHub-Backend\queryhub.db"

# First check if sqlite3.exe is available in PATH
$sqliteCmd = Get-Command sqlite3 -ErrorAction SilentlyContinue

if (-not $sqliteCmd) {
    Write-Host "SQLite command line tool not found. Let's try with PowerShell System.Data.SQLite..." -ForegroundColor Yellow
    
    # Try loading SQLite using .NET
    try {
        Add-Type -Path "System.Data.SQLite.dll" -ErrorAction Stop
        Write-Host "✓ System.Data.SQLite loaded" -ForegroundColor Green
    } catch {
        Write-Host "⚠ SQLite .NET library not available. Will verify through API only." -ForegroundColor Yellow
        
        # Fallback: Verify through API that the questions are persisted
        Write-Host "`nVerifying through API calls..." -ForegroundColor Yellow
        
        # Get questions again after a delay to ensure persistence
        Start-Sleep -Seconds 3
        $baseUrl = "http://localhost:5031/api"
        $questions = Invoke-RestMethod -Uri "$baseUrl/questions" -Method GET
        
        Write-Host "Total questions in database: $($questions.Count)"
        
        # Show the most recent questions (our test questions)
        $recentQuestions = $questions | Sort-Object id -Descending | Select-Object -First 3
        Write-Host "`nMost recent questions:"
        foreach ($q in $recentQuestions) {
            if ($q.title -like "*Database Persistence Test*") {
                Write-Host "✓ PERSISTENCE TEST QUESTION FOUND:" -ForegroundColor Green
            } else {
                Write-Host "  Other question:" -ForegroundColor Gray
            }
            Write-Host "    ID: $($q.id)" -ForegroundColor White
            Write-Host "    Title: $($q.title)" -ForegroundColor White
            Write-Host "    Created: $($q.createdAt)" -ForegroundColor White
            Write-Host ""
        }
        
        # Check specific test questions
        $testQuestions = $questions | Where-Object { $_.title -like "*Database Persistence Test*" }
        if ($testQuestions.Count -gt 0) {
            Write-Host "✅ SUCCESS: Found $($testQuestions.Count) test questions in the database!" -ForegroundColor Green
            Write-Host "This confirms that new questions are being properly persisted." -ForegroundColor Green
        } else {
            Write-Host "❌ FAILURE: No test questions found in database!" -ForegroundColor Red
        }
        
        return
    }
}

Write-Host "Using sqlite3 command line tool..." -ForegroundColor Green

# Query the database directly
$query = @"
SELECT 
    Id,
    Title,
    Body,
    UserId,
    CreatedAt
FROM Questions 
ORDER BY Id DESC 
LIMIT 5;
"@

Write-Host "`nExecuting SQL query to get latest questions..."
Write-Host "Query: $query"

try {
    $result = & sqlite3 $dbPath $query
    Write-Host "`nDirect database query results:" -ForegroundColor Yellow
    Write-Host $result
    
    if ($result -like "*Database Persistence Test*") {
        Write-Host "`n✅ SUCCESS: Test questions found in database!" -ForegroundColor Green
    } else {
        Write-Host "`n❌ FAILURE: Test questions not found in database!" -ForegroundColor Red
    }
} catch {
    Write-Host "Error executing SQL query: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n=== Verification Complete ===" -ForegroundColor Green
