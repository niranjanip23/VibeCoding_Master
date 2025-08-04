# Test script to reproduce the model mismatch error
# This script will test the complete flow: login -> ask question -> redirect to details

Write-Host "=== Testing Question Creation and Details Redirect ===" -ForegroundColor Green

# Step 1: Register a new user
Write-Host "`n1. Registering a new test user..." -ForegroundColor Yellow
$registerData = @{
    Username = "testuser_$(Get-Date -Format 'yyyyMMddHHmmss')"
    Email = "test_$(Get-Date -Format 'yyyyMMddHHmmss')@example.com"
    Password = "Test123!"
} | ConvertTo-Json

try {
    $registerResponse = Invoke-RestMethod -Uri "http://localhost:5031/api/auth/register" -Method POST -Body $registerData -ContentType "application/json"
    Write-Host "Registration successful: $($registerResponse | ConvertTo-Json)" -ForegroundColor Green
} catch {
    Write-Host "Registration failed: $($_.Exception.Message)" -ForegroundColor Red
    return
}

# Step 2: Use the token from registration
Write-Host "`n2. Using token from registration..." -ForegroundColor Yellow
$token = $registerResponse.token
Write-Host "Using token: $($token.Substring(0, 20))..." -ForegroundColor Green

# Step 3: Create a question via backend API directly
Write-Host "`n3. Creating question via backend API..." -ForegroundColor Yellow
$questionData = @{
    Title = "Test Question - Model Mismatch Test"
    Body = "This is a test question to reproduce the model mismatch error."
    Tags = @("test", "debugging")
} | ConvertTo-Json

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

try {
    $questionResponse = Invoke-RestMethod -Uri "http://localhost:5031/api/questions" -Method POST -Body $questionData -Headers $headers
    Write-Host "Question created successfully:" -ForegroundColor Green
    Write-Host ($questionResponse | ConvertTo-Json -Depth 3)
    $questionId = $questionResponse.id
} catch {
    Write-Host "Question creation failed: $($_.Exception.Message)" -ForegroundColor Red
    return
}

# Step 4: Test the frontend Details page directly
Write-Host "`n4. Testing frontend question details page..." -ForegroundColor Yellow
try {
    $detailsUrl = "http://localhost:5000/Questions/Details/$questionId"
    Write-Host "Attempting to access: $detailsUrl" -ForegroundColor Cyan
    
    # Try to get the page (this might fail due to authentication)
    $detailsResponse = Invoke-WebRequest -Uri $detailsUrl -Method GET -UseBasicParsing
    Write-Host "Details page accessible. Status: $($detailsResponse.StatusCode)" -ForegroundColor Green
} catch {
    Write-Host "Details page error: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        Write-Host "Response status: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    }
}

# Step 5: Test the backend question details API
Write-Host "`n5. Testing backend question details API..." -ForegroundColor Yellow
try {
    $backendDetailsResponse = Invoke-RestMethod -Uri "http://localhost:5031/api/questions/$questionId" -Method GET
    Write-Host "Backend question details retrieved successfully:" -ForegroundColor Green
    Write-Host ($backendDetailsResponse | ConvertTo-Json -Depth 3)
} catch {
    Write-Host "Backend question details failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n=== Test Complete ===" -ForegroundColor Green
Write-Host "Now try to access the frontend manually at: http://localhost:5000/Questions/Details/$questionId" -ForegroundColor Cyan
