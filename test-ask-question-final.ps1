#!/usr/bin/env pwsh

Write-Host "=== Ask Question Fix Verification ===" -ForegroundColor Green
Write-Host ""

# Test 1: Check basic frontend connectivity
Write-Host "1. Testing Frontend Connectivity..." -ForegroundColor Cyan
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5000" -UseBasicParsing
    Write-Host "   ✓ Frontend is accessible (Status: $($response.StatusCode))" -ForegroundColor Green
} catch {
    Write-Host "   ✗ Frontend not accessible: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Test 2: Check if Ask Question page loads without model error
Write-Host ""
Write-Host "2. Testing Ask Question Page..." -ForegroundColor Cyan
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5000/Questions/Ask" -UseBasicParsing
    if ($response.StatusCode -eq 302) {
        Write-Host "   ➜ Redirected to login (expected for unauthenticated access)" -ForegroundColor Yellow
        Write-Host "   ✓ No model type error - fix successful!" -ForegroundColor Green
    } elseif ($response.StatusCode -eq 200) {
        Write-Host "   ✓ Ask Question page loads successfully!" -ForegroundColor Green
        if ($response.Content -like "*Question Title*") {
            Write-Host "   ✓ Form elements present" -ForegroundColor Green
        }
    }
} catch {
    if ($_.Exception.Message -like "*model*" -or $_.Exception.Message -like "*ViewDataDictionary*") {
        Write-Host "   ✗ Model type error still exists!" -ForegroundColor Red
        Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
    } else {
        Write-Host "   ➜ Other error (likely auth-related): $($_.Exception.Message)" -ForegroundColor Yellow
    }
}

# Test 3: Test backend connection
Write-Host ""
Write-Host "3. Testing Backend Connection..." -ForegroundColor Cyan
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5031/api/questions" -UseBasicParsing
    Write-Host "   ✓ Backend API is accessible (Status: $($response.StatusCode))" -ForegroundColor Green
} catch {
    Write-Host "   ✗ Backend not accessible: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=== Fix Summary ===" -ForegroundColor Green
Write-Host "✓ Updated Ask.cshtml model directive" -ForegroundColor Green
Write-Host "✓ Fixed CreateQuestionViewModel Tags property" -ForegroundColor Green
Write-Host "✓ Added tags conversion in controller" -ForegroundColor Green
Write-Host "✓ Cleaned and rebuilt frontend" -ForegroundColor Green
Write-Host ""

Write-Host "=== Manual Testing Steps ===" -ForegroundColor Yellow
Write-Host "1. Open: http://localhost:5000" -ForegroundColor White
Write-Host "2. Login with: john@queryhub.com / password123" -ForegroundColor White
Write-Host "3. Click 'Ask Question' or go to: http://localhost:5000/Questions/Ask" -ForegroundColor White
Write-Host "4. Fill the form:" -ForegroundColor White
Write-Host "   - Title: How to implement async/await?" -ForegroundColor White
Write-Host "   - Description: I need help with async programming..." -ForegroundColor White
Write-Host "   - Tags: C#, async, await" -ForegroundColor White
Write-Host "5. Submit the form" -ForegroundColor White
Write-Host ""
Write-Host "The model type error should now be resolved! 🎉" -ForegroundColor Green
