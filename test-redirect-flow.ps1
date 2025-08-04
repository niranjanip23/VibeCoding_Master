# Test to reproduce the exact redirect issue by manually triggering the ask->details flow

Write-Host "=== Testing Exact Redirect Flow ===" -ForegroundColor Green

# First, create a question directly via backend to get a valid ID for testing redirects
$registerData = @{
    Username = "redirecttest_$(Get-Date -Format 'yyyyMMddHHmmss')"
    Email = "redirecttest_$(Get-Date -Format 'yyyyMMddHHmmss')@example.com"
    Password = "Test123!"
} | ConvertTo-Json

try {
    $registerResponse = Invoke-RestMethod -Uri "http://localhost:5031/api/auth/register" -Method POST -Body $registerData -ContentType "application/json"
    $token = $registerResponse.token
    Write-Host "User registered and token obtained" -ForegroundColor Green
} catch {
    Write-Host "Registration failed: $($_.Exception.Message)" -ForegroundColor Red
    return
}

# Create question via backend
$questionData = @{
    Title = "Redirect Test Question"
    Body = "Testing the redirect flow from ask to details"
    Tags = @("redirect", "test")
} | ConvertTo-Json

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

try {
    $questionResponse = Invoke-RestMethod -Uri "http://localhost:5031/api/questions" -Method POST -Body $questionData -Headers $headers
    $questionId = $questionResponse.id
    Write-Host "Question created with ID: $questionId" -ForegroundColor Green
} catch {
    Write-Host "Question creation failed: $($_.Exception.Message)" -ForegroundColor Red
    return
}

# Now test the frontend redirect by simulating what the Ask controller does
Write-Host "`nTesting frontend redirect simulation..." -ForegroundColor Yellow

# Test direct access to the details page (this should work)
try {
    $directResponse = Invoke-WebRequest -Uri "http://localhost:5000/Questions/Details/$questionId" -UseBasicParsing
    Write-Host "Direct access to details page: SUCCESS (Status: $($directResponse.StatusCode))" -ForegroundColor Green
} catch {
    Write-Host "Direct access FAILED: $($_.Exception.Message)" -ForegroundColor Red
}

# Test redirect via a custom URL that simulates the redirect
# This tests if there's any specific issue with the redirect flow
Write-Host "`nTesting redirect flow..." -ForegroundColor Yellow

try {
    # First, create a URL that would mimic what happens after successful question creation
    $redirectUrl = "http://localhost:5000/Questions/Details/$questionId"
    
    # Test with a follow-redirect approach (simulating browser behavior)
    $redirectResponse = Invoke-WebRequest -Uri $redirectUrl -UseBasicParsing -MaximumRedirection 5
    Write-Host "Redirect simulation: SUCCESS (Status: $($redirectResponse.StatusCode))" -ForegroundColor Green
    
    # Check if the response contains the debug info we added
    if ($redirectResponse.Content -match "Model Type: (.*?)<br/>") {
        $modelType = $matches[1]
        Write-Host "Model Type in response: $modelType" -ForegroundColor Cyan
    }
    
    if ($redirectResponse.Content -match "Model ID: (.*?)<br/>") {
        $modelId = $matches[1]
        Write-Host "Model ID in response: $modelId" -ForegroundColor Cyan
    }
    
} catch {
    Write-Host "Redirect test FAILED: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        Write-Host "HTTP Status: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    }
}

Write-Host "`n=== Redirect Flow Test Complete ===" -ForegroundColor Green
