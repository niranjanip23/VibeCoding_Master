# Test Self-View Prevention Script
# This script verifies that view counts only increment when viewed by non-authors

Write-Host "=== Testing Self-View Prevention ===" -ForegroundColor Green

# Configuration
$backendUrl = "http://localhost:5031"
$frontendUrl = "http://localhost:5001"

# Function to make API requests with authentication
function Invoke-AuthenticatedRequest {
    param(
        [string]$Url,
        [string]$Method = "GET",
        [hashtable]$Headers = @{},
        [string]$Body = $null
    )
    
    try {
        $params = @{
            Uri = $Url
            Method = $Method
            Headers = $Headers
            ContentType = "application/json"
        }
        
        if ($Body) {
            $params.Body = $Body
        }
        
        return Invoke-RestMethod @params
    }
    catch {
        Write-Host "Request failed: $($_.Exception.Message)" -ForegroundColor Red
        return $null
    }
}

# Function to login and get token
function Get-AuthToken {
    param([string]$Username, [string]$Password)
    
    $loginData = @{
        username = $Username
        password = $Password
    } | ConvertTo-Json
    
    $response = Invoke-AuthenticatedRequest -Url "$backendUrl/api/auth/login" -Method "POST" -Body $loginData
    
    if ($response -and $response.token) {
        return $response.token
    }
    return $null
}

# Test users (assuming they exist from previous tests)
$user1 = @{ username = "testuser1"; password = "TestPass123!" }
$user2 = @{ username = "testuser2"; password = "TestPass123!" }

Write-Host "`n1. Logging in test users..." -ForegroundColor Yellow

$token1 = Get-AuthToken -Username $user1.username -Password $user1.password
$token2 = Get-AuthToken -Username $user2.username -Password $user2.password

if (-not $token1 -or -not $token2) {
    Write-Host "Failed to authenticate users. Creating them first..." -ForegroundColor Yellow
    
    # Register users if they don't exist
    $registerData1 = @{
        username = $user1.username
        email = "testuser1@example.com"
        password = $user1.password
    } | ConvertTo-Json
    
    $registerData2 = @{
        username = $user2.username
        email = "testuser2@example.com"
        password = $user2.password
    } | ConvertTo-Json
    
    Invoke-AuthenticatedRequest -Url "$backendUrl/api/auth/register" -Method "POST" -Body $registerData1 | Out-Null
    Invoke-AuthenticatedRequest -Url "$backendUrl/api/auth/register" -Method "POST" -Body $registerData2 | Out-Null
    
    Start-Sleep -Seconds 2
    
    $token1 = Get-AuthToken -Username $user1.username -Password $user1.password
    $token2 = Get-AuthToken -Username $user2.username -Password $user2.password
}

if (-not $token1 -or -not $token2) {
    Write-Host "Still unable to authenticate users. Exiting." -ForegroundColor Red
    exit 1
}

Write-Host "✓ Both users authenticated successfully" -ForegroundColor Green

Write-Host "`n2. Creating a test question as user1..." -ForegroundColor Yellow

$questionData = @{
    title = "Self-View Test Question - $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
    body = "This is a test question to verify that self-views do not increment the view count."
    tags = @("test", "views")
} | ConvertTo-Json

$headers1 = @{ "Authorization" = "Bearer $token1" }
$newQuestion = Invoke-AuthenticatedRequest -Url "$backendUrl/api/questions" -Method "POST" -Headers $headers1 -Body $questionData

if (-not $newQuestion) {
    Write-Host "Failed to create test question" -ForegroundColor Red
    exit 1
}

$questionId = $newQuestion.id
Write-Host "✓ Created question with ID: $questionId" -ForegroundColor Green

Write-Host "`n3. Getting initial view count..." -ForegroundColor Yellow

$question = Invoke-AuthenticatedRequest -Url "$backendUrl/api/questions/$questionId"
$initialViews = $question.views
Write-Host "Initial view count: $initialViews" -ForegroundColor Cyan

Write-Host "`n4. User1 (author) views their own question..." -ForegroundColor Yellow

$headers1 = @{ "Authorization" = "Bearer $token1" }
$questionAfterSelfView = Invoke-AuthenticatedRequest -Url "$backendUrl/api/questions/$questionId" -Headers $headers1

$viewsAfterSelfView = $questionAfterSelfView.views
Write-Host "View count after self-view: $viewsAfterSelfView" -ForegroundColor Cyan

if ($viewsAfterSelfView -eq $initialViews) {
    Write-Host "✓ Self-view correctly did NOT increment view count" -ForegroundColor Green
} else {
    Write-Host "✗ Self-view incorrectly incremented view count" -ForegroundColor Red
}

Write-Host "`n5. User2 (different user) views the question..." -ForegroundColor Yellow

$headers2 = @{ "Authorization" = "Bearer $token2" }
$questionAfterOtherView = Invoke-AuthenticatedRequest -Url "$backendUrl/api/questions/$questionId" -Headers $headers2

$viewsAfterOtherView = $questionAfterOtherView.views
Write-Host "View count after other user view: $viewsAfterOtherView" -ForegroundColor Cyan

if ($viewsAfterOtherView -eq ($viewsAfterSelfView + 1)) {
    Write-Host "✓ Other user view correctly incremented view count" -ForegroundColor Green
} else {
    Write-Host "✗ Other user view did not increment view count as expected" -ForegroundColor Red
}

Write-Host "`n6. Anonymous user views the question..." -ForegroundColor Yellow

$questionAfterAnonymousView = Invoke-AuthenticatedRequest -Url "$backendUrl/api/questions/$questionId"

$viewsAfterAnonymousView = $questionAfterAnonymousView.views
Write-Host "View count after anonymous view: $viewsAfterAnonymousView" -ForegroundColor Cyan

if ($viewsAfterAnonymousView -eq ($viewsAfterOtherView + 1)) {
    Write-Host "✓ Anonymous view correctly incremented view count" -ForegroundColor Green
} else {
    Write-Host "✗ Anonymous view did not increment view count as expected" -ForegroundColor Red
}

Write-Host "`n7. User1 (author) views their question again..." -ForegroundColor Yellow

$questionAfterSecondSelfView = Invoke-AuthenticatedRequest -Url "$backendUrl/api/questions/$questionId" -Headers $headers1

$viewsAfterSecondSelfView = $questionAfterSecondSelfView.views
Write-Host "View count after second self-view: $viewsAfterSecondSelfView" -ForegroundColor Cyan

if ($viewsAfterSecondSelfView -eq $viewsAfterAnonymousView) {
    Write-Host "✓ Second self-view correctly did NOT increment view count" -ForegroundColor Green
} else {
    Write-Host "✗ Second self-view incorrectly incremented view count" -ForegroundColor Red
}

Write-Host "`n=== Test Summary ===" -ForegroundColor Green
Write-Host "Initial views: $initialViews"
Write-Host "After self-view: $viewsAfterSelfView (should be same as initial)"
Write-Host "After other user view: $viewsAfterOtherView (should be +1)"
Write-Host "After anonymous view: $viewsAfterAnonymousView (should be +1)"
Write-Host "After second self-view: $viewsAfterSecondSelfView (should be same as anonymous)"

$totalExpectedViews = $initialViews + 2  # Only other user and anonymous should increment
if ($viewsAfterSecondSelfView -eq $totalExpectedViews) {
    Write-Host "`n✓ ALL TESTS PASSED - Self-view prevention is working correctly!" -ForegroundColor Green
} else {
    Write-Host "`n✗ Some tests failed - Self-view prevention needs attention" -ForegroundColor Red
}

Write-Host "`nTest completed." -ForegroundColor Yellow
