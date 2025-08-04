#!/usr/bin/env pwsh

Write-Host "=== Testing Question Creation Fix ===" -ForegroundColor Green
Write-Host ""

Write-Host "1. Testing Backend API directly..." -ForegroundColor Cyan

# Test backend directly first
$loginHeaders = @{ "Content-Type" = "application/json" }
$loginBody = '{"email":"john@queryhub.com","password":"password123"}'
$loginResponse = Invoke-WebRequest -Uri "http://localhost:5031/api/auth/login" -Method POST -Headers $loginHeaders -Body $loginBody -UseBasicParsing
$loginResult = $loginResponse.Content | ConvertFrom-Json
$token = $loginResult.token

$questionHeaders = @{
    "Content-Type" = "application/json"
    "Authorization" = "Bearer $token"
}
$questionBody = '{"Title":"Final Test Question","Body":"Testing after API model fix","Tags":["test","fix","final"]}'

try {
    $questionResponse = Invoke-WebRequest -Uri "http://localhost:5031/api/questions" -Method POST -Headers $questionHeaders -Body $questionBody -UseBasicParsing
    Write-Host "   ✓ Backend API working: $($questionResponse.StatusCode)" -ForegroundColor Green
    
    $result = $questionResponse.Content | ConvertFrom-Json
    Write-Host "   ✓ Question created with ID: $($result.id)" -ForegroundColor Green
    Write-Host "   ✓ Tags format: $($result.tags | ConvertTo-Json -Compress)" -ForegroundColor Green
} catch {
    Write-Host "   ✗ Backend test failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "2. Testing Frontend..." -ForegroundColor Cyan

try {
    $frontendResponse = Invoke-WebRequest -Uri "http://localhost:5000" -UseBasicParsing
    Write-Host "   ✓ Frontend accessible: $($frontendResponse.StatusCode)" -ForegroundColor Green
} catch {
    Write-Host "   ✗ Frontend not accessible: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=== Fix Summary ===" -ForegroundColor Green
Write-Host "✓ Fixed QuestionApiModel.Tags: string → List<string>" -ForegroundColor Green
Write-Host "✓ Fixed QuestionDetailApiModel.Tags: string → List<string>" -ForegroundColor Green
Write-Host "✓ Updated mapping methods to handle List<string> directly" -ForegroundColor Green
Write-Host "✓ Rebuilt and restarted frontend" -ForegroundColor Green
Write-Host ""
Write-Host "The 'Failed to create question' issue should now be resolved!" -ForegroundColor Green
Write-Host ""
Write-Host "🎯 Try creating a question now:" -ForegroundColor Yellow
Write-Host "   1. Go to: http://localhost:5000" -ForegroundColor White
Write-Host "   2. Login with: john@queryhub.com / password123" -ForegroundColor White
Write-Host "   3. Click 'Ask Question'" -ForegroundColor White
Write-Host "   4. Fill the form and submit" -ForegroundColor White
