#!/usr/bin/env pwsh

Write-Host "=== Testing Question Creation Flow ===" -ForegroundColor Green
Write-Host ""

# Step 1: Login and get token
Write-Host "1. Logging in..." -ForegroundColor Cyan
$loginHeaders = @{ "Content-Type" = "application/json" }
$loginBody = @{
    email = "john@queryhub.com"
    password = "password123"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-WebRequest -Uri "http://localhost:5031/api/auth/login" -Method POST -Headers $loginHeaders -Body $loginBody -UseBasicParsing
    $loginResult = $loginResponse.Content | ConvertFrom-Json
    $token = $loginResult.token
    Write-Host "   ✓ Login successful, got token" -ForegroundColor Green
} catch {
    Write-Host "   ✗ Login failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 2: Test question creation (exact frontend format)
Write-Host ""
Write-Host "2. Creating question..." -ForegroundColor Cyan
$questionHeaders = @{
    "Content-Type" = "application/json"
    "Authorization" = "Bearer $token"
}

$createRequest = @{
    Title = "How to debug ASP.NET Core issues?"
    Body = "I'm having trouble debugging my ASP.NET Core application. Can someone help me understand the best practices for troubleshooting?"
    Tags = @("aspnet", "debugging", "troubleshooting")
}
$questionJson = $createRequest | ConvertTo-Json

Write-Host "   Request: $questionJson" -ForegroundColor Yellow

try {
    $questionResponse = Invoke-WebRequest -Uri "http://localhost:5031/api/questions" -Method POST -Headers $questionHeaders -Body $questionJson -UseBasicParsing
    
    Write-Host "   ✓ Question created successfully!" -ForegroundColor Green
    Write-Host "   Status: $($questionResponse.StatusCode)" -ForegroundColor Green
    
    $createdQuestion = $questionResponse.Content | ConvertFrom-Json
    Write-Host "   Question ID: $($createdQuestion.id)" -ForegroundColor Green
    Write-Host "   Title: $($createdQuestion.title)" -ForegroundColor Green
    Write-Host "   Tags: $($createdQuestion.tags -join ', ')" -ForegroundColor Green
    
} catch {
    Write-Host "   ✗ Question creation failed!" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
    
    if ($_.Exception.Response) {
        $statusCode = $_.Exception.Response.StatusCode
        Write-Host "   Status Code: $statusCode" -ForegroundColor Red
        
        try {
            $errorStream = $_.Exception.Response.GetResponseStream()
            $reader = New-Object System.IO.StreamReader($errorStream)
            $errorContent = $reader.ReadToEnd()
            Write-Host "   Error Details: $errorContent" -ForegroundColor Red
        } catch {
            Write-Host "   Could not read error details" -ForegroundColor Red
        }
    }
}

# Step 3: Verify question was created
Write-Host ""
Write-Host "3. Verifying question list..." -ForegroundColor Cyan
try {
    $questionsResponse = Invoke-WebRequest -Uri "http://localhost:5031/api/questions" -UseBasicParsing
    $questions = $questionsResponse.Content | ConvertFrom-Json
    
    Write-Host "   ✓ Total questions now: $($questions.Count)" -ForegroundColor Green
    
    # Show the most recent question
    $mostRecent = $questions | Sort-Object { [datetime]$_.createdAt } -Descending | Select-Object -First 1
    if ($mostRecent) {
        Write-Host "   Most recent: '$($mostRecent.title)' by User $($mostRecent.userId)" -ForegroundColor Green
    }
    
} catch {
    Write-Host "   ✗ Could not verify questions: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=== Test Complete ===" -ForegroundColor Green
