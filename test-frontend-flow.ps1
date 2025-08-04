# Test script to simulate the exact frontend flow that causes the error
Write-Host "=== Testing Frontend Question Creation Flow ===" -ForegroundColor Green

# We'll use a session-aware web client to simulate the browser behavior
$session = New-Object Microsoft.PowerShell.Commands.WebRequestSession

# Step 1: Get the login page to establish session
Write-Host "`n1. Getting login page to establish session..." -ForegroundColor Yellow
try {
    $loginPageResponse = Invoke-WebRequest -Uri "http://localhost:5000/Account/Login" -WebSession $session -UseBasicParsing
    Write-Host "Login page loaded successfully" -ForegroundColor Green
} catch {
    Write-Host "Failed to load login page: $($_.Exception.Message)" -ForegroundColor Red
    return
}

# Step 2: Login via frontend form
Write-Host "`n2. Logging in via frontend..." -ForegroundColor Yellow

# Create a test user first via backend
$registerData = @{
    Username = "frontendtest_$(Get-Date -Format 'yyyyMMddHHmmss')"
    Email = "frontendtest_$(Get-Date -Format 'yyyyMMddHHmmss')@example.com"
    Password = "Test123!"
} | ConvertTo-Json

try {
    $registerResponse = Invoke-RestMethod -Uri "http://localhost:5031/api/auth/register" -Method POST -Body $registerData -ContentType "application/json"
    Write-Host "User registered: $($registerResponse.username)" -ForegroundColor Green
    $username = $registerResponse.username
} catch {
    Write-Host "User registration failed: $($_.Exception.Message)" -ForegroundColor Red
    return
}

# Parse the login page to get the anti-forgery token
$loginPageContent = $loginPageResponse.Content
if ($loginPageContent -match 'name="__RequestVerificationToken" type="hidden" value="([^"]+)"') {
    $antiForgeryToken = $matches[1]
    Write-Host "Anti-forgery token extracted: $($antiForgeryToken.Substring(0, 20))..." -ForegroundColor Green
} else {
    Write-Host "Could not extract anti-forgery token" -ForegroundColor Red
    return
}

# Login via frontend form
$loginFormData = @{
    Username = $username
    Password = "Test123!"
    __RequestVerificationToken = $antiForgeryToken
}

try {
    $loginResponse = Invoke-WebRequest -Uri "http://localhost:5000/Account/Login" -Method POST -Body $loginFormData -WebSession $session -UseBasicParsing
    if ($loginResponse.StatusCode -eq 302 -or $loginResponse.BaseResponse.ResponseUri -match "Home") {
        Write-Host "Frontend login successful" -ForegroundColor Green
    } else {
        Write-Host "Login might have failed - Status: $($loginResponse.StatusCode)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "Frontend login failed: $($_.Exception.Message)" -ForegroundColor Red
    # Continue anyway, might be a redirect issue
}

# Step 3: Get the Ask Question page
Write-Host "`n3. Getting Ask Question page..." -ForegroundColor Yellow
try {
    $askPageResponse = Invoke-WebRequest -Uri "http://localhost:5000/Questions/Ask" -WebSession $session -UseBasicParsing
    Write-Host "Ask page loaded successfully" -ForegroundColor Green
    
    # Extract anti-forgery token from Ask page
    $askPageContent = $askPageResponse.Content
    if ($askPageContent -match 'name="__RequestVerificationToken" type="hidden" value="([^"]+)"') {
        $askAntiForgeryToken = $matches[1]
        Write-Host "Ask page anti-forgery token extracted" -ForegroundColor Green
    } else {
        Write-Host "Could not extract anti-forgery token from Ask page" -ForegroundColor Red
        return
    }
} catch {
    Write-Host "Failed to load Ask page: $($_.Exception.Message)" -ForegroundColor Red
    return
}

# Step 4: Submit question via frontend form
Write-Host "`n4. Submitting question via frontend form..." -ForegroundColor Yellow
$questionFormData = @{
    Title = "Frontend Test Question - Model Mismatch Debug"
    Description = "This question is posted via the frontend form to test the model mismatch error."
    Tags = "frontend,test,debugging"
    __RequestVerificationToken = $askAntiForgeryToken
}

try {
    $questionResponse = Invoke-WebRequest -Uri "http://localhost:5000/Questions/Ask" -Method POST -Body $questionFormData -WebSession $session -MaximumRedirection 0 -ErrorAction SilentlyContinue
    
    Write-Host "Question submission response:" -ForegroundColor Cyan
    Write-Host "Status Code: $($questionResponse.StatusCode)" -ForegroundColor Cyan
    Write-Host "Location Header: $($questionResponse.Headers.Location)" -ForegroundColor Cyan
    
    if ($questionResponse.StatusCode -eq 302 -and $questionResponse.Headers.Location) {
        $redirectUrl = $questionResponse.Headers.Location
        Write-Host "Redirecting to: $redirectUrl" -ForegroundColor Green
        
        # Follow the redirect manually to see if this causes the error
        Write-Host "`n5. Following redirect to Details page..." -ForegroundColor Yellow
        try {
            $detailsResponse = Invoke-WebRequest -Uri $redirectUrl -WebSession $session -UseBasicParsing
            Write-Host "Details page loaded successfully after redirect" -ForegroundColor Green
            Write-Host "Details page status: $($detailsResponse.StatusCode)" -ForegroundColor Green
        } catch {
            Write-Host "ERROR: Details page failed after redirect!" -ForegroundColor Red
            Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
            if ($_.Exception.Response) {
                Write-Host "Response Status: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
            }
        }
    } else {
        Write-Host "No redirect received or question creation failed" -ForegroundColor Red
    }
} catch {
    Write-Host "Question submission failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n=== Frontend Flow Test Complete ===" -ForegroundColor Green
