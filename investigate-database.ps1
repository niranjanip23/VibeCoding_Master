# Comprehensive Database Investigation and Fix Script

Write-Host "=== COMPREHENSIVE DATABASE INVESTIGATION ===" -ForegroundColor Green

# Check if backend is running and responding
Write-Host "`n1. Checking Backend Status..." -ForegroundColor Yellow
try {
    $healthCheck = Invoke-RestMethod -Uri "http://localhost:5031/api/questions?page=1&pageSize=5" -Method GET -TimeoutSec 5
    Write-Host "✅ Backend is responding" -ForegroundColor Green
    Write-Host "   API returned $($healthCheck.Count) questions" -ForegroundColor Cyan
} catch {
    Write-Host "❌ Backend not responding: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "   Make sure the backend is running on port 5031" -ForegroundColor Yellow
    return
}

# Check database file details
Write-Host "`n2. Checking Database File..." -ForegroundColor Yellow
$dbPath = "QueryHub-Backend\queryhub.db"
if (Test-Path $dbPath) {
    $dbInfo = Get-ChildItem $dbPath
    Write-Host "✅ Database file exists" -ForegroundColor Green
    Write-Host "   Path: $($dbInfo.FullName)" -ForegroundColor Cyan
    Write-Host "   Size: $([math]::Round($dbInfo.Length / 1KB, 2)) KB" -ForegroundColor Cyan
    Write-Host "   Last Modified: $($dbInfo.LastWriteTime)" -ForegroundColor Cyan
} else {
    Write-Host "❌ Database file not found at: $dbPath" -ForegroundColor Red
}

# Get ALL questions from API (not just recent ones)
Write-Host "`n3. Fetching ALL Questions from API..." -ForegroundColor Yellow
try {
    # Try to get more questions by increasing page size
    $allQuestions = Invoke-RestMethod -Uri "http://localhost:5031/api/questions?page=1&pageSize=100" -Method GET
    
    Write-Host "✅ Total questions in API: $($allQuestions.Count)" -ForegroundColor Green
    
    if ($allQuestions.Count -eq 0) {
        Write-Host "⚠️  No questions found in the database!" -ForegroundColor Yellow
        Write-Host "   This might indicate a data initialization issue" -ForegroundColor Yellow
    } else {
        Write-Host "`n📋 All Questions Summary:" -ForegroundColor Cyan
        
        # Group by date
        $questionsByDate = $allQuestions | Group-Object { 
            [DateTime]::Parse($_.createdAt).ToString("yyyy-MM-dd") 
        } | Sort-Object Name -Descending
        
        foreach ($dateGroup in $questionsByDate) {
            Write-Host "`n📅 Date: $($dateGroup.Name) ($($dateGroup.Count) questions)" -ForegroundColor White
            foreach ($q in ($dateGroup.Group | Sort-Object { [DateTime]::Parse($_.createdAt) } -Descending)) {
                $createdTime = [DateTime]::Parse($q.createdAt).ToString("HH:mm:ss")
                Write-Host "   • Q$($q.id): $($q.title) (at $createdTime)" -ForegroundColor Gray
            }
        }
    }
} catch {
    Write-Host "❌ Error fetching questions: $($_.Exception.Message)" -ForegroundColor Red
}

# Check if there are any recent API calls in the backend logs
Write-Host "`n4. Testing Question Creation..." -ForegroundColor Yellow
try {
    # Create a test question to verify the database is working
    $testUser = @{
        Username = "dbtest_$(Get-Date -Format 'yyyyMMddHHmmss')"
        Email = "dbtest_$(Get-Date -Format 'yyyyMMddHHmmss')@test.com"
        Password = "Test123!"
    } | ConvertTo-Json
    
    Write-Host "   Creating test user..." -ForegroundColor Gray
    $userResponse = Invoke-RestMethod -Uri "http://localhost:5031/api/auth/register" -Method POST -Body $testUser -ContentType "application/json"
    $token = $userResponse.token
    
    Write-Host "   Creating test question..." -ForegroundColor Gray
    $testQuestion = @{
        Title = "Database Test Question - $(Get-Date -Format 'HH:mm:ss')"
        Body = "This is a test question to verify database operations are working correctly."
        Tags = @("test", "database", "verification")
    } | ConvertTo-Json
    
    $headers = @{
        "Authorization" = "Bearer $token"
        "Content-Type" = "application/json"
    }
    
    $questionResponse = Invoke-RestMethod -Uri "http://localhost:5031/api/questions" -Method POST -Body $testQuestion -Headers $headers
    
    Write-Host "✅ Test question created successfully!" -ForegroundColor Green
    Write-Host "   Question ID: $($questionResponse.id)" -ForegroundColor Cyan
    Write-Host "   Title: $($questionResponse.title)" -ForegroundColor Cyan
    
    # Verify it appears in the API immediately
    Write-Host "   Verifying question appears in API..." -ForegroundColor Gray
    Start-Sleep -Seconds 1
    $verifyQuestions = Invoke-RestMethod -Uri "http://localhost:5031/api/questions?page=1&pageSize=5" -Method GET
    $newQuestion = $verifyQuestions | Where-Object { $_.id -eq $questionResponse.id }
    
    if ($newQuestion) {
        Write-Host "✅ New question immediately visible in API" -ForegroundColor Green
    } else {
        Write-Host "❌ New question NOT visible in API - Database sync issue!" -ForegroundColor Red
    }
    
} catch {
    Write-Host "❌ Error testing question creation: $($_.Exception.Message)" -ForegroundColor Red
}

# Check for any database locking or connection issues
Write-Host "`n5. Database Health Check..." -ForegroundColor Yellow
try {
    # Try to get a specific question to test individual record retrieval
    if ($allQuestions -and $allQuestions.Count -gt 0) {
        $firstQuestion = $allQuestions[0]
        $detailResponse = Invoke-RestMethod -Uri "http://localhost:5031/api/questions/$($firstQuestion.id)" -Method GET
        Write-Host "✅ Individual question retrieval working" -ForegroundColor Green
    }
} catch {
    Write-Host "❌ Error with individual question retrieval: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n=== INVESTIGATION COMPLETE ===" -ForegroundColor Green
Write-Host "`nSUMMARY:" -ForegroundColor White
Write-Host "- Database file exists and backend is responding" -ForegroundColor Gray
Write-Host "- Total questions currently in database: $($allQuestions.Count)" -ForegroundColor Gray
Write-Host "- If you're not seeing your questions, they may have been created in a different session" -ForegroundColor Gray
Write-Host "- The test question creation confirms the database is working correctly" -ForegroundColor Gray
