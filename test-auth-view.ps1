# Test Authentication and Self-View Prevention
Write-Host "=== Testing Self-View Prevention with Authentication ===" -ForegroundColor Green

$backendUrl = "http://localhost:5031"

# Test login
$loginData = @{
    username = "testuser1"
    password = "TestPass123!"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "$backendUrl/api/auth/login" -Method POST -Body $loginData -ContentType "application/json"
    $token = $loginResponse.token
    Write-Host "✓ Login successful" -ForegroundColor Green
    
    # Get a question the user didn't create
    $questions = Invoke-RestMethod -Uri "$backendUrl/api/questions" -Method GET
    $otherUserQuestion = $questions | Where-Object { $_.userId -ne $loginResponse.user.id } | Select-Object -First 1
    
    if ($otherUserQuestion) {
        $questionId = $otherUserQuestion.id
        $initialViews = $otherUserQuestion.views
        
        Write-Host "Testing with question ID: $questionId (created by user $($otherUserQuestion.userId))" -ForegroundColor Yellow
        Write-Host "Current user ID: $($loginResponse.user.id)" -ForegroundColor Yellow
        Write-Host "Initial view count: $initialViews" -ForegroundColor Cyan
        
        # View with authentication (should increment since not the author)
        $headers = @{ "Authorization" = "Bearer $token" }
        $questionAfterView = Invoke-RestMethod -Uri "$backendUrl/api/questions/$questionId" -Method GET -Headers $headers
        $viewsAfterAuth = $questionAfterView.views
        
        Write-Host "View count after authenticated view: $viewsAfterAuth" -ForegroundColor Cyan
        
        if ($viewsAfterAuth -eq ($initialViews + 1)) {
            Write-Host "✓ Authenticated view by non-author correctly incremented view count" -ForegroundColor Green
        } else {
            Write-Host "✗ Authenticated view by non-author did not increment as expected" -ForegroundColor Red
        }
    }
    
    # Now test with a question by the same user
    $userQuestion = $questions | Where-Object { $_.userId -eq $loginResponse.user.id } | Select-Object -First 1
    
    if ($userQuestion) {
        $questionId = $userQuestion.id
        $initialViews = $userQuestion.views
        
        Write-Host "`nTesting self-view with question ID: $questionId (created by user $($userQuestion.userId))" -ForegroundColor Yellow
        Write-Host "Current user ID: $($loginResponse.user.id)" -ForegroundColor Yellow
        Write-Host "Initial view count: $initialViews" -ForegroundColor Cyan
        
        # Self-view with authentication (should NOT increment)
        $headers = @{ "Authorization" = "Bearer $token" }
        $questionAfterSelfView = Invoke-RestMethod -Uri "$backendUrl/api/questions/$questionId" -Method GET -Headers $headers
        $viewsAfterSelfView = $questionAfterSelfView.views
        
        Write-Host "View count after self-view: $viewsAfterSelfView" -ForegroundColor Cyan
        
        if ($viewsAfterSelfView -eq $initialViews) {
            Write-Host "✓ Self-view correctly did NOT increment view count" -ForegroundColor Green
        } else {
            Write-Host "✗ Self-view incorrectly incremented view count" -ForegroundColor Red
        }
    } else {
        Write-Host "No questions found for the current user to test self-view" -ForegroundColor Yellow
    }
}
catch {
    Write-Host "✗ Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`nTest completed." -ForegroundColor Yellow
