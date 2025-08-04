# Database Persistence Test Script
# This script tests whether new questions are properly persisted in the database

$baseUrl = "http://localhost:5031/api"
$currentTime = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"

Write-Host "=== Database Persistence Test ===" -ForegroundColor Green
Write-Host "Testing question creation and database persistence..."

# Step 1: Check current question count
Write-Host "`n1. Checking current questions..." -ForegroundColor Yellow
try {
    $currentQuestions = Invoke-RestMethod -Uri "$baseUrl/questions" -Method GET
    $initialCount = $currentQuestions.Count
    Write-Host "   Current questions count: $initialCount"
    Write-Host "   Latest question ID: $($currentQuestions[0].id)"
} catch {
    Write-Host "   Error getting current questions: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 2: Register a test user
Write-Host "`n2. Registering test user..." -ForegroundColor Yellow
$testUser = @{
    Email = "test_$currentTime@example.com"
    Username = "testuser_$currentTime"
    Password = "TestPassword123!"
} | ConvertTo-Json

try {
    $registerResponse = Invoke-RestMethod -Uri "$baseUrl/auth/register" -Method POST -Body $testUser -ContentType "application/json"
    Write-Host "   User registered successfully: $($registerResponse.email)"
} catch {
    Write-Host "   Registration error: $($_.Exception.Message)" -ForegroundColor Red
    # Continue anyway in case user already exists
}

# Step 3: Login to get token
Write-Host "`n3. Logging in..." -ForegroundColor Yellow
$loginData = @{
    Email = "test_$currentTime@example.com"
    Password = "TestPassword123!"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method POST -Body $loginData -ContentType "application/json"
    $token = $loginResponse.token
    Write-Host "   Login successful. Token received."
} catch {
    Write-Host "   Login error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 4: Create a new question
Write-Host "`n4. Creating new question..." -ForegroundColor Yellow
$newQuestion = @{
    Title = "Database Persistence Test - $currentTime"
    Content = "This is a test question to verify database persistence functionality. Created at $currentTime"
    Tags = @("test", "database", "persistence")
} | ConvertTo-Json

$headers = @{
    'Authorization' = "Bearer $token"
    'Content-Type' = 'application/json'
}

try {
    $createResponse = Invoke-RestMethod -Uri "$baseUrl/questions" -Method POST -Body $newQuestion -Headers $headers
    $newQuestionId = $createResponse.id
    Write-Host "   Question created successfully!"
    Write-Host "   New question ID: $newQuestionId"
    Write-Host "   Title: $($createResponse.title)"
} catch {
    Write-Host "   Error creating question: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 5: Verify question appears in API
Write-Host "`n5. Verifying question in API..." -ForegroundColor Yellow
Start-Sleep -Seconds 2
try {
    $updatedQuestions = Invoke-RestMethod -Uri "$baseUrl/questions" -Method GET
    $newCount = $updatedQuestions.Count
    $questionFound = $updatedQuestions | Where-Object { $_.id -eq $newQuestionId }
    
    Write-Host "   Updated questions count: $newCount"
    Write-Host "   Count increased by: $($newCount - $initialCount)"
    
    if ($questionFound) {
        Write-Host "   ✓ New question found in API response!" -ForegroundColor Green
        Write-Host "   Title: $($questionFound.title)"
        Write-Host "   ID: $($questionFound.id)"
    } else {
        Write-Host "   ✗ New question NOT found in API response!" -ForegroundColor Red
    }
} catch {
    Write-Host "   Error verifying question in API: $($_.Exception.Message)" -ForegroundColor Red
}

# Step 6: Check database file directly
Write-Host "`n6. Checking database file..." -ForegroundColor Yellow
$dbPath = "c:\Users\2317697\OneDrive - Cognizant\Desktop\CC\VibeCoding_Master-1\QueryHub-Backend\queryhub.db"

try {
    if (Test-Path $dbPath) {
        $dbInfo = Get-Item $dbPath
        Write-Host "   Database file exists: $dbPath"
        Write-Host "   Last modified: $($dbInfo.LastWriteTime)"
        Write-Host "   Size: $($dbInfo.Length) bytes"
        
        # Check if the file was modified recently (within last 5 minutes)
        $fiveMinutesAgo = (Get-Date).AddMinutes(-5)
        if ($dbInfo.LastWriteTime -gt $fiveMinutesAgo) {
            Write-Host "   ✓ Database file was recently modified!" -ForegroundColor Green
        } else {
            Write-Host "   ⚠ Database file was not recently modified." -ForegroundColor Yellow
            Write-Host "   This might indicate the data wasn't persisted to disk."
        }
    } else {
        Write-Host "   ✗ Database file not found!" -ForegroundColor Red
    }
} catch {
    Write-Host "   Error checking database file: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n=== Test Complete ===" -ForegroundColor Green
Write-Host "Summary:"
Write-Host "- Initial question count: $initialCount"
Write-Host "- Final question count: $newCount"
Write-Host "- New question ID: $newQuestionId"
Write-Host "- Question found in API: $(if ($questionFound) { 'YES' } else { 'NO' })"
