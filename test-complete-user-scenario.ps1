# Complete View Count Test - User Scenario
# This script tests the complete user scenario: view question details → go back to home → see updated view count

Write-Host "=== Complete View Count Update Test ===" -ForegroundColor Green
Write-Host "Testing the exact user scenario: view question → back to home → check if view count updated" -ForegroundColor Yellow
Write-Host ""

# Step 1: Get initial view count from questions list (home page data source)
Write-Host "Step 1: Getting initial view count from questions API..." -ForegroundColor Cyan
try {
    $questionsResponse = Invoke-RestMethod -Uri "http://localhost:5031/api/questions" -Method GET
    $testQuestion = $questionsResponse | Where-Object { $_.title -like "*Real-time Profile Test*" } | Select-Object -First 1
    
    if (-not $testQuestion) {
        $testQuestion = $questionsResponse[0]  # Use first question if test question not found
    }
    
    $questionId = $testQuestion.id
    $questionTitle = $testQuestion.title
    $initialViews = $testQuestion.views
    
    Write-Host "   Question ID: $questionId" -ForegroundColor White
    Write-Host "   Title: $questionTitle" -ForegroundColor White
    Write-Host "   Initial Views (from questions list): $initialViews" -ForegroundColor White
} catch {
    Write-Host "   ❌ Error getting questions list: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 2: Simulate user viewing question details (this should increment view count)
Write-Host ""
Write-Host "Step 2: Simulating user viewing question details..." -ForegroundColor Cyan
try {
    $questionDetailsResponse = Invoke-RestMethod -Uri "http://localhost:5031/api/questions/$questionId" -Method GET
    $viewsAfterDetails = $questionDetailsResponse.views
    Write-Host "   Views after viewing details: $viewsAfterDetails" -ForegroundColor White
    
    if ($viewsAfterDetails -gt $initialViews) {
        Write-Host "   ✅ View count incremented correctly" -ForegroundColor Green
    } else {
        Write-Host "   ❌ View count did not increment!" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "   ❌ Error viewing question details: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 3: Simulate user going back to home page (get questions list again)
Write-Host ""
Write-Host "Step 3: Simulating user going back to home page..." -ForegroundColor Cyan
try {
    Start-Sleep 1  # Small delay to ensure any caching clears
    $questionsResponse2 = Invoke-RestMethod -Uri "http://localhost:5031/api/questions" -Method GET
    $testQuestionUpdated = $questionsResponse2 | Where-Object { $_.id -eq $questionId }
    $finalViews = $testQuestionUpdated.views
    
    Write-Host "   Views in questions list after going back: $finalViews" -ForegroundColor White
    
    if ($finalViews -eq $viewsAfterDetails) {
        Write-Host "   ✅ Home page shows updated view count!" -ForegroundColor Green
    } else {
        Write-Host "   ❌ Home page still shows old view count!" -ForegroundColor Red
        Write-Host "   Expected: $viewsAfterDetails, Got: $finalViews" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "   ❌ Error getting updated questions list: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 4: Repeat the cycle once more to ensure consistency
Write-Host ""
Write-Host "Step 4: Testing consistency with another view..." -ForegroundColor Cyan
try {
    $beforeSecondView = $finalViews
    $secondDetailsResponse = Invoke-RestMethod -Uri "http://localhost:5031/api/questions/$questionId" -Method GET
    $afterSecondDetails = $secondDetailsResponse.views
    
    $questionsResponse3 = Invoke-RestMethod -Uri "http://localhost:5031/api/questions" -Method GET
    $testQuestionFinal = $questionsResponse3 | Where-Object { $_.id -eq $questionId }
    $afterSecondHome = $testQuestionFinal.views
    
    Write-Host "   Before 2nd view: $beforeSecondView" -ForegroundColor White
    Write-Host "   After 2nd details view: $afterSecondDetails" -ForegroundColor White
    Write-Host "   After 2nd home page check: $afterSecondHome" -ForegroundColor White
    
    if ($afterSecondDetails -eq $beforeSecondView + 1 -and $afterSecondHome -eq $afterSecondDetails) {
        Write-Host "   ✅ Consistency check passed!" -ForegroundColor Green
    } else {
        Write-Host "   ❌ Consistency check failed!" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "   ❌ Error in consistency test: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Final Summary
Write-Host ""
Write-Host "=== TEST RESULTS SUMMARY ===" -ForegroundColor Green
Write-Host "Question: $questionTitle" -ForegroundColor White
Write-Host "Initial Views: $initialViews" -ForegroundColor White
Write-Host "After 1st Details View: $viewsAfterDetails" -ForegroundColor White
Write-Host "After 1st Home Page Check: $finalViews" -ForegroundColor White
Write-Host "After 2nd Details View: $afterSecondDetails" -ForegroundColor White
Write-Host "After 2nd Home Page Check: $afterSecondHome" -ForegroundColor White
Write-Host ""
Write-Host "✅ VIEW COUNT UPDATE ISSUE IS FULLY RESOLVED!" -ForegroundColor Green
Write-Host ""
Write-Host "User Scenario Validation:" -ForegroundColor Cyan
Write-Host "  1. ✅ User views question details → View count increments" -ForegroundColor Green
Write-Host "  2. ✅ User goes back to home page → Updated view count is displayed" -ForegroundColor Green
Write-Host "  3. ✅ Process is consistent and repeatable" -ForegroundColor Green
Write-Host ""
Write-Host "The frontend at http://localhost:5000 will now show updated view counts!" -ForegroundColor Yellow
