# Complete Accept Answer Test Script
# Tests both the functionality and UI feedback

Write-Host "=== Accept Answer Complete Test ===" -ForegroundColor Green

$frontendUrl = "http://localhost:5000"
$backendUrl = "http://localhost:5031/api"

# Step 1: Test Backend API
Write-Host "`n1. Testing Backend Accept API..." -ForegroundColor Yellow

try {
    # Login
    $loginData = '{"Email": "john@queryhub.com", "Password": "password123"}'
    $loginResponse = Invoke-RestMethod -Uri "$backendUrl/auth/login" -Method POST -Body $loginData -ContentType "application/json"
    Write-Host "   ✅ Login successful: $($loginResponse.username)" -ForegroundColor Green
    
    $token = $loginResponse.token
    $headers = @{ "Authorization" = "Bearer $token" }
    
    # Get questions with answers
    $questions = Invoke-RestMethod -Uri "$backendUrl/questions" -Method GET -Headers $headers
    $questionWithAnswers = $questions | Where-Object { $_.answers -and $_.answers.Count -gt 0 } | Select-Object -First 1
    
    if ($questionWithAnswers) {
        Write-Host "   📝 Found test question: '$($questionWithAnswers.title)'" -ForegroundColor Cyan
        $answerId = $questionWithAnswers.answers[0].id
        Write-Host "   🎯 Testing answer ID: $answerId" -ForegroundColor Cyan
        
        # Test accept answer API
        $acceptResponse = Invoke-RestMethod -Uri "$backendUrl/answers/$answerId/accept" -Method POST -Headers $headers
        Write-Host "   ✅ Answer accepted successfully!" -ForegroundColor Green
        Write-Host "   📄 Response: $($acceptResponse.body)" -ForegroundColor White
    } else {
        Write-Host "   ⚠️  No questions with answers found" -ForegroundColor Yellow
    }
    
} catch {
    Write-Host "   ❌ Backend API test failed: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.ErrorDetails) {
        Write-Host "   Details: $($_.ErrorDetails.Message)" -ForegroundColor Red
    }
}

# Step 2: Test Frontend UI
Write-Host "`n2. Testing Frontend UI..." -ForegroundColor Yellow

try {
    $frontendResponse = Invoke-WebRequest -Uri "$frontendUrl" -UseBasicParsing
    if ($frontendResponse.StatusCode -eq 200) {
        Write-Host "   ✅ Frontend is accessible" -ForegroundColor Green
        
        # Check for JavaScript changes
        $scriptResponse = Invoke-WebRequest -Uri "$frontendUrl/js/site.js" -UseBasicParsing
        if ($scriptResponse.Content -match "response\.message") {
            Write-Host "   ✅ JavaScript updated to show server messages" -ForegroundColor Green
        } else {
            Write-Host "   ⚠️  JavaScript may not show custom messages" -ForegroundColor Yellow
        }
    }
} catch {
    Write-Host "   ❌ Frontend test failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Step 3: Provide manual testing instructions
Write-Host "`n3. Manual Testing Instructions:" -ForegroundColor Yellow
Write-Host "   1. Open: $frontendUrl/Account/Login" -ForegroundColor Cyan
Write-Host "   2. Login with: john@queryhub.com / password123" -ForegroundColor Cyan
Write-Host "   3. Go to: $frontendUrl/Questions/Details/1" -ForegroundColor Cyan
Write-Host "   4. Look for Accept Answer button (check mark icon)" -ForegroundColor Cyan
Write-Host "   5. Click to accept - should show message with points info" -ForegroundColor Cyan
Write-Host "   6. Check profile at: $frontendUrl/Account/Profile" -ForegroundColor Cyan
Write-Host "   7. Points should update after page refresh" -ForegroundColor Cyan

Write-Host "`n=== Test Complete ===" -ForegroundColor Green
Write-Host "✨ Changes implemented:" -ForegroundColor White
Write-Host "   • Real AcceptAnswerAsync API call added" -ForegroundColor White
Write-Host "   • Custom success message about bonus points" -ForegroundColor White
Write-Host "   • Error handling for unauthorized acceptance" -ForegroundColor White
Write-Host "   • Backend awards 15 reputation points to answer author" -ForegroundColor White
Write-Host "   • UI shows green checkmark and Accepted Answer header" -ForegroundColor White
