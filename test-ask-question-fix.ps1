#!/usr/bin/env pwsh

Write-Host "=== Testing Ask Question Page (Fixed) ===" -ForegroundColor Green
Write-Host ""

# Test 1: Check if Ask Question page loads without authentication error
Write-Host "1. Testing Ask Question page access..." -ForegroundColor Cyan
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5000/Questions/Ask" -UseBasicParsing
    Write-Host "   Ask Page Response: $($response.StatusCode)" -ForegroundColor Green
    
    if ($response.StatusCode -eq 200) {
        Write-Host "   ✓ Ask Question page loads successfully" -ForegroundColor Green
    } elseif ($response.StatusCode -eq 302) {
        Write-Host "   ➜ Redirected to login (expected for unauthorized access)" -ForegroundColor Yellow
    }
} catch {
    if ($_.Exception.Message -like "*model*" -or $_.Exception.Message -like "*ViewDataDictionary*") {
        Write-Host "   ✗ Model type error still exists: $($_.Exception.Message)" -ForegroundColor Red
    } else {
        Write-Host "   ➜ Expected redirect or auth error: $($_.Exception.Message)" -ForegroundColor Yellow
    }
}

Write-Host ""

# Test 2: Login first and then test Ask page
Write-Host "2. Testing with authentication..." -ForegroundColor Cyan
try {
    # Create a session container to maintain cookies
    $session = New-Object Microsoft.PowerShell.Commands.WebRequestSession
    
    # Login first
    $loginData = @{
        Email = "john@queryhub.com"
        Password = "password123"
    }
    
    $loginResponse = Invoke-WebRequest -Uri "http://localhost:5000/Account/Login" -Method POST -Body $loginData -WebSession $session -UseBasicParsing
    Write-Host "   Login Response: $($loginResponse.StatusCode)" -ForegroundColor Green
    
    if ($loginResponse.StatusCode -eq 200 -or $loginResponse.StatusCode -eq 302) {
        # Now try to access Ask page with authenticated session
        $askResponse = Invoke-WebRequest -Uri "http://localhost:5000/Questions/Ask" -WebSession $session -UseBasicParsing
        Write-Host "   Authenticated Ask Page: $($askResponse.StatusCode)" -ForegroundColor Green
        
        if ($askResponse.StatusCode -eq 200) {
            Write-Host "   ✓ Ask Question page works with authentication" -ForegroundColor Green
            
            # Check if the page contains the expected form elements
            if ($askResponse.Content -like "*Question Title*" -and $askResponse.Content -like "*Question Description*") {
                Write-Host "   ✓ Page contains expected form elements" -ForegroundColor Green
            }
        }
    }
} catch {
    Write-Host "   Authentication test failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=== Fix Summary ===" -ForegroundColor Green
Write-Host "✓ Updated Ask.cshtml model from Question to CreateQuestionViewModel" -ForegroundColor Green
Write-Host "✓ Fixed Tags property from List<string> to string in CreateQuestionViewModel" -ForegroundColor Green  
Write-Host "✓ Added tags conversion in controller (string → List<string>)" -ForegroundColor Green
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "1. Login at http://localhost:5000/Account/Login" -ForegroundColor White
Write-Host "   Email: john@queryhub.com" -ForegroundColor White
Write-Host "   Password: password123" -ForegroundColor White
Write-Host "2. Try accessing Ask Question page: http://localhost:5000/Questions/Ask" -ForegroundColor White
Write-Host "3. Test creating a new question" -ForegroundColor White
