# Simple Database Persistence Test
$baseUrl = "http://localhost:5031/api"
$currentTime = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"

Write-Host "=== Database Persistence Test ===" -ForegroundColor Green

# Step 1: Check current questions
Write-Host "`n1. Getting current questions..."
$currentQuestions = Invoke-RestMethod -Uri "$baseUrl/questions" -Method GET
$initialCount = $currentQuestions.Count
Write-Host "Current questions count: $initialCount"

# Step 2: Register user
Write-Host "`n2. Registering test user..."
$testUser = @{
    Email = "test_$currentTime@example.com"
    Username = "testuser_$currentTime"
    Password = "TestPassword123!"
} | ConvertTo-Json

$registerResponse = Invoke-RestMethod -Uri "$baseUrl/auth/register" -Method POST -Body $testUser -ContentType "application/json"
Write-Host "User registered: $($registerResponse.email)"

# Step 3: Login
Write-Host "`n3. Logging in..."
$loginData = @{
    Email = "test_$currentTime@example.com"
    Password = "TestPassword123!"
} | ConvertTo-Json

$loginResponse = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method POST -Body $loginData -ContentType "application/json"
$token = $loginResponse.token
Write-Host "Login successful"

# Step 4: Create question
Write-Host "`n4. Creating new question..."
$newQuestion = @{
    Title = "Database Persistence Test - $currentTime"
    Content = "This is a test question to verify database persistence functionality."
    Tags = @("test", "database", "persistence")
} | ConvertTo-Json

$headers = @{
    'Authorization' = "Bearer $token"
    'Content-Type' = 'application/json'
}

$createResponse = Invoke-RestMethod -Uri "$baseUrl/questions" -Method POST -Body $newQuestion -Headers $headers
$newQuestionId = $createResponse.id
Write-Host "Question created with ID: $newQuestionId"

# Step 5: Verify in API
Write-Host "`n5. Verifying question in API..."
Start-Sleep -Seconds 2
$updatedQuestions = Invoke-RestMethod -Uri "$baseUrl/questions" -Method GET
$newCount = $updatedQuestions.Count
$questionFound = $updatedQuestions | Where-Object { $_.id -eq $newQuestionId }

Write-Host "Updated questions count: $newCount"
Write-Host "Count increased by: $($newCount - $initialCount)"

if ($questionFound) {
    Write-Host "✓ New question found in API!" -ForegroundColor Green
    Write-Host "Title: $($questionFound.title)"
} else {
    Write-Host "✗ New question NOT found in API!" -ForegroundColor Red
}

# Step 6: Check database file
Write-Host "`n6. Checking database file..."
$dbPath = "c:\Users\2317697\OneDrive - Cognizant\Desktop\CC\VibeCoding_Master-1\QueryHub-Backend\queryhub.db"

if (Test-Path $dbPath) {
    $dbInfo = Get-Item $dbPath
    Write-Host "Database file exists"
    Write-Host "Last modified: $($dbInfo.LastWriteTime)"
    Write-Host "Size: $($dbInfo.Length) bytes"
    
    $fiveMinutesAgo = (Get-Date).AddMinutes(-5)
    if ($dbInfo.LastWriteTime -gt $fiveMinutesAgo) {
        Write-Host "✓ Database file was recently modified!" -ForegroundColor Green
    } else {
        Write-Host "⚠ Database file was not recently modified." -ForegroundColor Yellow
    }
} else {
    Write-Host "✗ Database file not found!" -ForegroundColor Red
}

Write-Host "`n=== Test Summary ===" -ForegroundColor Green
Write-Host "Initial count: $initialCount"
Write-Host "Final count: $newCount"
Write-Host "Question ID: $newQuestionId"
Write-Host "Found in API: $(if ($questionFound) { 'YES' } else { 'NO' })"
