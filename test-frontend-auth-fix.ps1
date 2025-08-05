# Test Frontend Authentication Flow
Write-Host "=== Testing Frontend Authentication Fix ===" -ForegroundColor Green

$frontendUrl = "https://localhost:5001"
$backendUrl = "http://localhost:5031"

# First test: Direct backend login (should work)
Write-Host "`n1. Testing direct backend login..." -ForegroundColor Yellow
try {
    $loginResponse = Invoke-RestMethod -Uri "$backendUrl/api/auth/login" -Method POST -Body '{"email":"john@queryhub.com","password":"password123"}' -ContentType "application/json"
    Write-Host "✓ Direct backend login successful - User ID: $($loginResponse.userId)" -ForegroundColor Green
    
    # Test question 21 with proper auth
    $headers = @{ "Authorization" = "Bearer $($loginResponse.token)" }
    $beforeView = Invoke-RestMethod -Uri "$backendUrl/api/questions/21" -Method GET -Headers $headers
    Write-Host "Question 21 views with proper auth: $($beforeView.views)" -ForegroundColor Cyan
    
} catch {
    Write-Host "✗ Direct backend login failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Second test: Check if question 21 gets incremented without auth (should increment)
Write-Host "`n2. Testing anonymous access (should increment)..." -ForegroundColor Yellow
try {
    $beforeAnonymous = Invoke-RestMethod -Uri "$backendUrl/api/questions/21" -Method GET
    Write-Host "Question 21 views (anonymous): $($beforeAnonymous.views)" -ForegroundColor Cyan
} catch {
    Write-Host "✗ Anonymous access failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n3. Instructions for manual frontend testing:" -ForegroundColor Yellow
Write-Host "   a. Go to: $frontendUrl" -ForegroundColor Cyan
Write-Host "   b. Login with: john@queryhub.com / password123" -ForegroundColor Cyan  
Write-Host "   c. Navigate to: $frontendUrl/Questions/Details/21" -ForegroundColor Cyan
Write-Host "   d. Check backend logs for authentication status" -ForegroundColor Cyan

Write-Host "`nTest completed. Check the backend logs for authentication details." -ForegroundColor Green
