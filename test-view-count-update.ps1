# Test View Count Update Issue
# This script tests if view counts update properly on the home page after viewing a question

Write-Host "=== Testing View Count Update Issue ===" -ForegroundColor Green
Write-Host ""

# Step 1: Get initial view count from home page
Write-Host "Step 1: Getting initial view count from home page..." -ForegroundColor Yellow
$homePageResponse = Invoke-RestMethod -Uri "http://localhost:5000" -Method GET
Write-Host "Home page loaded successfully" -ForegroundColor Green

# Step 2: Get initial view count from API
Write-Host ""
Write-Host "Step 2: Getting initial view count from API..." -ForegroundColor Yellow
try {
    $questionsResponse = Invoke-RestMethod -Uri "http://localhost:5031/api/questions" -Method GET
    $firstQuestion = $questionsResponse[0]
    $initialViews = $firstQuestion.views
    $questionId = $firstQuestion.id
    $questionTitle = $firstQuestion.title
    
    Write-Host "Question ID: $questionId" -ForegroundColor Cyan
    Write-Host "Question Title: $questionTitle" -ForegroundColor Cyan
    Write-Host "Initial Views: $initialViews" -ForegroundColor Cyan
} catch {
    Write-Host "Error getting questions from API: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 3: View the question details (this should increment view count)
Write-Host ""
Write-Host "Step 3: Viewing question details to increment view count..." -ForegroundColor Yellow
try {
    $questionDetailsResponse = Invoke-RestMethod -Uri "http://localhost:5031/api/questions/$questionId" -Method GET
    $viewsAfterDetails = $questionDetailsResponse.views
    Write-Host "Views after viewing details page: $viewsAfterDetails" -ForegroundColor Cyan
    
    if ($viewsAfterDetails -gt $initialViews) {
        Write-Host "✅ View count incremented successfully (was $initialViews, now $viewsAfterDetails)" -ForegroundColor Green
    } else {
        Write-Host "❌ View count did not increment!" -ForegroundColor Red
    }
} catch {
    Write-Host "Error viewing question details: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 4: Check view count from API again (should show updated count)
Write-Host ""
Write-Host "Step 4: Checking view count from API after increment..." -ForegroundColor Yellow
try {
    Start-Sleep 1  # Small delay to ensure database commit
    $questionsResponse2 = Invoke-RestMethod -Uri "http://localhost:5031/api/questions" -Method GET
    $questionAfter = $questionsResponse2 | Where-Object { $_.id -eq $questionId }
    $finalViews = $questionAfter.views
    
    Write-Host "Final Views from API: $finalViews" -ForegroundColor Cyan
    
    if ($finalViews -eq $viewsAfterDetails) {
        Write-Host "✅ API shows correct updated view count" -ForegroundColor Green
    } else {
        Write-Host "❌ API view count inconsistency! Expected: $viewsAfterDetails, Got: $finalViews" -ForegroundColor Red
    }
} catch {
    Write-Host "Error getting updated questions from API: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 5: Test if home page shows updated view count
Write-Host ""
Write-Host "Step 5: Testing if home page shows updated view count..." -ForegroundColor Yellow
Write-Host "NOTE: Home page is HTML, so we're testing the backend API that powers it" -ForegroundColor Gray

# Summary
Write-Host ""
Write-Host "=== TEST SUMMARY ===" -ForegroundColor Green
Write-Host "Initial Views: $initialViews" -ForegroundColor White
Write-Host "Views After Details: $viewsAfterDetails" -ForegroundColor White
Write-Host "Final API Views: $finalViews" -ForegroundColor White
Write-Host ""

if ($finalViews -gt $initialViews) {
    Write-Host "✅ View count functionality is working correctly!" -ForegroundColor Green
    Write-Host "If home page doesn't show updated count, it's a frontend caching issue." -ForegroundColor Yellow
} else {
    Write-Host "❌ View count functionality has issues that need to be fixed!" -ForegroundColor Red
}

Write-Host ""
Write-Host "=== NEXT STEPS ===" -ForegroundColor Blue
Write-Host "1. Manually check home page at http://localhost:5000" -ForegroundColor White
Write-Host "2. Look for question: $questionTitle" -ForegroundColor White
Write-Host "3. Verify if view count shows: $finalViews" -ForegroundColor White
Write-Host "4. If not, the issue is frontend caching or data fetching" -ForegroundColor White
