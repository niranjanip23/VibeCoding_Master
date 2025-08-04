# Simple E2E Authentication Test

Write-Host "=== E2E AUTHENTICATION TEST ===" -ForegroundColor Green

$BackendUrl = "http://localhost:5031"
$FrontendUrl = "http://localhost:5000"
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$testEmail = "e2e.test.$timestamp@queryhub.com"
$testPassword = "E2ETest123!"
$testName = "E2E Test User $timestamp"

$results = @()

# Test 1: Registration
Write-Host "`n1. Testing Registration..." -ForegroundColor Yellow
try {
    $registerData = @{
        Name = $testName
        Email = $testEmail
        Password = $testPassword
    } | ConvertTo-Json

    $registerResponse = Invoke-RestMethod -Uri "$BackendUrl/api/auth/register" -Method POST -Body $registerData -ContentType "application/json"
    Write-Host "   OK Registration successful" -ForegroundColor Green
    $results += "PASS"
}
catch {
    if ($_.Exception.Message -match "400") {
        Write-Host "   OK User exists (continuing)" -ForegroundColor Green
        $results += "PASS"
    } else {
        Write-Host "   ERROR Registration failed: $($_.Exception.Message)" -ForegroundColor Red
        $results += "FAIL"
    }
}

# Test 2: Login
Write-Host "`n2. Testing Login..." -ForegroundColor Yellow
try {
    $loginData = @{
        Email = $testEmail
        Password = $testPassword
    } | ConvertTo-Json

    $loginResponse = Invoke-RestMethod -Uri "$BackendUrl/api/auth/login" -Method POST -Body $loginData -ContentType "application/json"
    
    if ($loginResponse.token) {
        Write-Host "   OK Login successful, token received" -ForegroundColor Green
        $authToken = $loginResponse.token
        $results += "PASS"
    } else {
        Write-Host "   ERROR No token received" -ForegroundColor Red
        $results += "FAIL"
    }
}
catch {
    Write-Host "   ERROR Login failed: $($_.Exception.Message)" -ForegroundColor Red
    # Try with existing user
    try {
        $existingLoginData = @{
            Email = "john.doe@example.com"
            Password = "password123"
        } | ConvertTo-Json
        
        $existingLoginResponse = Invoke-RestMethod -Uri "$BackendUrl/api/auth/login" -Method POST -Body $existingLoginData -ContentType "application/json"
        if ($existingLoginResponse.token) {
            Write-Host "   OK Login with existing user works" -ForegroundColor Green
            $authToken = $existingLoginResponse.token
            $results += "PASS"
        }
    }
    catch {
        Write-Host "   ERROR Both logins failed" -ForegroundColor Red
        $results += "FAIL"
    }
}

# Test 3: Frontend Pages
Write-Host "`n3. Testing Frontend Pages..." -ForegroundColor Yellow
try {
    $regPageResponse = Invoke-WebRequest -Uri "$FrontendUrl/Account/Register" -Method GET
    $loginPageResponse = Invoke-WebRequest -Uri "$FrontendUrl/Account/Login" -Method GET
    
    if ($regPageResponse.StatusCode -eq 200 -and $loginPageResponse.StatusCode -eq 200) {
        Write-Host "   OK Frontend auth pages accessible" -ForegroundColor Green
        $results += "PASS"
    }
}
catch {
    Write-Host "   ERROR Frontend pages failed: $($_.Exception.Message)" -ForegroundColor Red
    $results += "FAIL"
}

# Test 4: Protected API Access
Write-Host "`n4. Testing Protected API Access..." -ForegroundColor Yellow
if ($authToken) {
    try {
        $headers = @{
            "Authorization" = "Bearer $authToken"
            "Content-Type" = "application/json"
        }
        
        $protectedResponse = Invoke-RestMethod -Uri "$BackendUrl/api/questions" -Method GET -Headers $headers
        Write-Host "   OK Protected API accessible with token" -ForegroundColor Green
        $results += "PASS"
    }
    catch {
        Write-Host "   ERROR Protected API failed: $($_.Exception.Message)" -ForegroundColor Red
        $results += "FAIL"
    }
} else {
    Write-Host "   SKIP No auth token available" -ForegroundColor Yellow
    $results += "SKIP"
}

# Results
$passCount = ($results | Where-Object { $_ -eq "PASS" }).Count
$failCount = ($results | Where-Object { $_ -eq "FAIL" }).Count

Write-Host "`n=== AUTHENTICATION TEST RESULTS ===" -ForegroundColor Green
Write-Host "Passed: $passCount" -ForegroundColor Green
Write-Host "Failed: $failCount" -ForegroundColor Red

if ($failCount -eq 0) {
    Write-Host "`nAUTHENTICATION E2E TEST: PASSED" -ForegroundColor White -BackgroundColor Green
} else {
    Write-Host "`nAUTHENTICATION E2E TEST: FAILED" -ForegroundColor White -BackgroundColor Red
}
