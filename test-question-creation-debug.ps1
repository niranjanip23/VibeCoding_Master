# Test question creation with detailed logging - debug version
$baseUrl = "http://localhost:5031"
$frontendUrl = "http://localhost:5000"

Write-Host "=== Testing Question Creation with Debug Logging ===" -ForegroundColor Green

# 1. Register a test user
Write-Host "`n1. Registering test user..." -ForegroundColor Yellow
$registerData = @{
    Username = "testuser_debug"
    Email = "testuser_debug@example.com"
    Password = "TestPassword123!"
} | ConvertTo-Json

try {
    $registerResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/register" -Method POST -Body $registerData -ContentType "application/json"
    Write-Host "Registration successful: $($registerResponse.Username)" -ForegroundColor Green
    $token = $registerResponse.Token
} catch {
    Write-Host "Registration failed: $($_.Exception.Message)" -ForegroundColor Red
    
    # Try to login with existing user
    Write-Host "Trying to login instead..." -ForegroundColor Yellow
    $loginData = @{
        Email = "testuser_debug@example.com"
        Password = "TestPassword123!"
    } | ConvertTo-Json
    
    try {
        $loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method POST -Body $loginData -ContentType "application/json"
        Write-Host "Login successful: $($loginResponse.Username)" -ForegroundColor Green
        $token = $loginResponse.Token
    } catch {
        Write-Host "Login also failed: $($_.Exception.Message)" -ForegroundColor Red
        exit 1
    }
}

Write-Host "Token: $token" -ForegroundColor Cyan

# 2. Create a question via backend API
Write-Host "`n2. Creating question via backend API..." -ForegroundColor Yellow
$questionData = @{
    Title = "Debug Test Question - $(Get-Date -Format 'HH:mm:ss')"
    Body = "This is a test question to debug the frontend integration issue."
    Tags = @("debugging", "frontend", "integration")
} | ConvertTo-Json

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/questions" -Method POST -Body $questionData -Headers $headers
    Write-Host "Backend question creation successful!" -ForegroundColor Green
    Write-Host "Question ID: $($response.Id)" -ForegroundColor Cyan
    Write-Host "Title: $($response.Title)" -ForegroundColor Cyan
    Write-Host "Created: $($response.CreatedAt)" -ForegroundColor Cyan
    Write-Host "Tags: $($response.Tags -join ', ')" -ForegroundColor Cyan
    Write-Host "Full Response:" -ForegroundColor Cyan
    $response | ConvertTo-Json -Depth 3 | Write-Host
} catch {
    Write-Host "Backend question creation failed: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseText = $reader.ReadToEnd()
        Write-Host "Response: $responseText" -ForegroundColor Red
    }
    exit 1
}

Write-Host "`n=== Backend API test completed successfully ===" -ForegroundColor Green
Write-Host "Now you can test the frontend UI manually and check the logs." -ForegroundColor Yellow
Write-Host "Frontend URL: $frontendUrl" -ForegroundColor Cyan
