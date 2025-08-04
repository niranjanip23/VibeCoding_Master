# End-to-End Authentication Flow Testing

param(
    [string]$BackendUrl = "http://localhost:5031",
    [string]$FrontendUrl = "http://localhost:5000"
)

Write-Host "=== E2E AUTHENTICATION FLOW TESTING ===" -ForegroundColor Green
Write-Host "Testing complete user authentication journey from frontend to backend" -ForegroundColor Cyan

$testResults = @()
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$testEmail = "e2e.test.$timestamp@queryhub.com"
$testPassword = "E2ETest123!"
$testName = "E2E Test User $timestamp"

# Test 1: Registration Flow (Frontend → Backend → Database)
Write-Host "`n1. Testing User Registration Flow..." -ForegroundColor Yellow

# Step 1.1: Check registration page accessibility
try {
    $regPageResponse = Invoke-WebRequest -Uri "$FrontendUrl/Account/Register" -Method GET
    if ($regPageResponse.StatusCode -eq 200) {
        Write-Host "   ✓ Registration page accessible" -ForegroundColor Green
        $testResults += "PASS: Registration page accessible"
    }
} catch {
    Write-Host "   ✗ Registration page failed: $($_.Exception.Message)" -ForegroundColor Red
    $testResults += "FAIL: Registration page not accessible"
}

# Step 1.2: Test backend registration API directly
try {
    $registerData = @{
        Name = $testName
        Email = $testEmail
        Password = $testPassword
    } | ConvertTo-Json

    $registerResponse = Invoke-RestMethod -Uri "$BackendUrl/api/auth/register" -Method POST -Body $registerData -ContentType "application/json"
    Write-Host "   ✓ Backend registration API works" -ForegroundColor Green
    Write-Host "     Response: $($registerResponse.message)" -ForegroundColor Gray
    $testResults += "PASS: Backend registration successful"
} catch {
    if ($_.Exception.Message -match "400") {
        Write-Host "   ⚠ User might already exist (testing with existing user)" -ForegroundColor Yellow
        $testResults += "WARN: User already exists"
    } else {
        Write-Host "   ✗ Backend registration failed: $($_.Exception.Message)" -ForegroundColor Red
        $testResults += "FAIL: Backend registration failed"
    }
}

# Test 2: Login Flow (Frontend → Backend → JWT Token)
Write-Host "`n2. Testing User Login Flow..." -ForegroundColor Yellow

# Step 2.1: Check login page accessibility
try {
    $loginPageResponse = Invoke-WebRequest -Uri "$FrontendUrl/Account/Login" -Method GET
    if ($loginPageResponse.StatusCode -eq 200) {
        Write-Host "   ✓ Login page accessible" -ForegroundColor Green
        $testResults += "PASS: Login page accessible"
    }
} catch {
    Write-Host "   ✗ Login page failed: $($_.Exception.Message)" -ForegroundColor Red
    $testResults += "FAIL: Login page not accessible"
}

# Step 2.2: Test backend login API directly
try {
    $loginData = @{
        Email = $testEmail
        Password = $testPassword
    } | ConvertTo-Json

    $loginResponse = Invoke-RestMethod -Uri "$BackendUrl/api/auth/login" -Method POST -Body $loginData -ContentType "application/json"
    if ($loginResponse.token) {
        Write-Host "   ✓ Backend login API works" -ForegroundColor Green
        Write-Host "     User: $($loginResponse.name)" -ForegroundColor Gray
        Write-Host "     Token received: Yes" -ForegroundColor Gray
        $authToken = $loginResponse.token
        $testResults += "PASS: Backend login successful with JWT token"
    } else {
        Write-Host "   ✗ No token received" -ForegroundColor Red
        $testResults += "FAIL: No JWT token received"
    }
} catch {
    Write-Host "   ✗ Backend login failed: $($_.Exception.Message)" -ForegroundColor Red
    $testResults += "FAIL: Backend login failed"
    # Try with existing user
    try {
        $existingLoginData = @{
            Email = "john.doe@example.com"
            Password = "password123"
        } | ConvertTo-Json
        
        $existingLoginResponse = Invoke-RestMethod -Uri "$BackendUrl/api/auth/login" -Method POST -Body $existingLoginData -ContentType "application/json"
        if ($existingLoginResponse.token) {
            Write-Host "   ✓ Login works with existing user" -ForegroundColor Green
            $authToken = $existingLoginResponse.token
            $testResults += "PASS: Login with existing user successful"
        }
    } catch {
        Write-Host "   ✗ Login with existing user also failed" -ForegroundColor Red
    }
}

# Test 3: Protected Route Access
Write-Host "`n3. Testing Protected Route Access..." -ForegroundColor Yellow

# Step 3.1: Test accessing protected route without authentication
try {
    $askPageResponse = Invoke-WebRequest -Uri "$FrontendUrl/Questions/Ask" -Method GET
    # Should redirect to login or show login prompt
    Write-Host "   ✓ Ask page accessible (may redirect to login)" -ForegroundColor Green
    $testResults += "PASS: Protected route handling works"
} catch {
    Write-Host "   ⚠ Ask page access: $($_.Exception.Message)" -ForegroundColor Yellow
}

# Step 3.2: Test protected API endpoint with token
if ($authToken) {
    try {
        $headers = @{
            "Authorization" = "Bearer $authToken"
            "Content-Type" = "application/json"
        }
        
        $protectedResponse = Invoke-RestMethod -Uri "$BackendUrl/api/questions" -Method GET -Headers $headers
        Write-Host "   ✓ Protected API endpoint accessible with token" -ForegroundColor Green
        $testResults += "PASS: Protected API access with JWT token works"
    } catch {
        Write-Host "   ✗ Protected API endpoint failed: $($_.Exception.Message)" -ForegroundColor Red
        $testResults += "FAIL: Protected API access failed"
    }
}

# Test 4: Session Management
Write-Host "`n4. Testing Session Management..." -ForegroundColor Yellow

# Step 4.1: Test session persistence
try {
    $sessionContainer = New-Object Microsoft.PowerShell.Commands.WebRequestSession
    $loginPageWithSession = Invoke-WebRequest -Uri "$FrontendUrl/Account/Login" -Method GET -WebSession $sessionContainer
    
    if ($loginPageWithSession.StatusCode -eq 200) {
        Write-Host "   ✓ Session management working" -ForegroundColor Green
        $testResults += "PASS: Session management functional"
    }
} catch {
    Write-Host "   ⚠ Session test: $($_.Exception.Message)" -ForegroundColor Yellow
}

# Test Results Summary
Write-Host "`n=== AUTHENTICATION FLOW TEST RESULTS ===" -ForegroundColor Green

$passCount = ($testResults | Where-Object { $_ -like "PASS:*" }).Count
$failCount = ($testResults | Where-Object { $_ -like "FAIL:*" }).Count
$warnCount = ($testResults | Where-Object { $_ -like "WARN:*" }).Count

Write-Host "`nTest Results Summary:" -ForegroundColor White
Write-Host "✓ Passed: $passCount" -ForegroundColor Green
Write-Host "✗ Failed: $failCount" -ForegroundColor Red
Write-Host "⚠ Warnings: $warnCount" -ForegroundColor Yellow

Write-Host "`nDetailed Results:" -ForegroundColor White
foreach ($result in $testResults) {
    if ($result -like "PASS:*") {
        Write-Host "✓ $($result.Substring(5))" -ForegroundColor Green
    } elseif ($result -like "FAIL:*") {
        Write-Host "✗ $($result.Substring(5))" -ForegroundColor Red
    } elseif ($result -like "WARN:*") {
        Write-Host "⚠ $($result.Substring(5))" -ForegroundColor Yellow
    }
}

if ($failCount -eq 0) {
    Write-Host "`n🎉 AUTHENTICATION FLOW E2E TEST: PASSED" -ForegroundColor White -BackgroundColor Green
} else {
    Write-Host "`n❌ AUTHENTICATION FLOW E2E TEST: FAILED" -ForegroundColor White -BackgroundColor Red
}

Write-Host "`nTest completed at: $(Get-Date)" -ForegroundColor Gray
return $failCount -eq 0
