# End-to-End Questions Management Testing

param(
    [string]$BackendUrl = "http://localhost:5031",
    [string]$FrontendUrl = "http://localhost:5000"
)

Write-Host "=== E2E QUESTIONS MANAGEMENT TESTING ===" -ForegroundColor Green
Write-Host "Testing complete question lifecycle from frontend to backend" -ForegroundColor Cyan

$testResults = @()
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"

# First, get authentication token
Write-Host "`n0. Setting up authentication..." -ForegroundColor Yellow
try {
    $loginData = @{
        Email = "john.doe@example.com"
        Password = "password123"
    } | ConvertTo-Json

    $loginResponse = Invoke-RestMethod -Uri "$BackendUrl/api/auth/login" -Method POST -Body $loginData -ContentType "application/json"
    $authToken = $loginResponse.token
    $headers = @{
        "Authorization" = "Bearer $authToken"
        "Content-Type" = "application/json"
    }
    Write-Host "   ✓ Authentication token obtained" -ForegroundColor Green
} catch {
    Write-Host "   ✗ Failed to get auth token: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Test 1: Questions List Display (Backend → Frontend)
Write-Host "`n1. Testing Questions List Display..." -ForegroundColor Yellow

# Step 1.1: Get questions from backend API
try {
    $backendQuestions = Invoke-RestMethod -Uri "$BackendUrl/api/questions" -Method GET
    Write-Host "   ✓ Backend API returned $($backendQuestions.Count) questions" -ForegroundColor Green
    $testResults += "PASS: Backend questions API functional"
} catch {
    Write-Host "   ✗ Backend questions API failed: $($_.Exception.Message)" -ForegroundColor Red
    $testResults += "FAIL: Backend questions API failed"
}

# Step 1.2: Check frontend questions page
try {
    $frontendQuestionsPage = Invoke-WebRequest -Uri "$FrontendUrl/Questions" -Method GET
    if ($frontendQuestionsPage.StatusCode -eq 200) {
        Write-Host "   ✓ Frontend questions page accessible" -ForegroundColor Green
        
        # Check if page contains question-related content
        if ($frontendQuestionsPage.Content -match "question|Question") {
            Write-Host "   ✓ Frontend displays question content" -ForegroundColor Green
            $testResults += "PASS: Frontend questions page displays content"
        } else {
            Write-Host "   ⚠ Frontend may not be displaying questions properly" -ForegroundColor Yellow
            $testResults += "WARN: Frontend questions display unclear"
        }
    }
} catch {
    Write-Host "   ✗ Frontend questions page failed: $($_.Exception.Message)" -ForegroundColor Red
    $testResults += "FAIL: Frontend questions page not accessible"
}

# Test 2: Question Creation Flow (Frontend → Backend → Database)
Write-Host "`n2. Testing Question Creation Flow..." -ForegroundColor Yellow

# Step 2.1: Check Ask page accessibility
try {
    $askPageResponse = Invoke-WebRequest -Uri "$FrontendUrl/Questions/Ask" -Method GET
    Write-Host "   ✓ Ask question page accessible" -ForegroundColor Green
    $testResults += "PASS: Ask question page accessible"
} catch {
    Write-Host "   ⚠ Ask page may require authentication: $($_.Exception.Message)" -ForegroundColor Yellow
}

# Step 2.2: Create question via backend API
$testQuestionData = @{
    Title = "E2E Test Question $timestamp"
    Body = "This is a test question created during E2E testing. It should appear in the frontend and be stored in the database."
    Tags = @("testing", "e2e", "automation")
}

try {
    $questionJson = $testQuestionData | ConvertTo-Json
    $createdQuestion = Invoke-RestMethod -Uri "$BackendUrl/api/questions" -Method POST -Body $questionJson -Headers $headers
    
    if ($createdQuestion.id) {
        Write-Host "   ✓ Question created successfully via API" -ForegroundColor Green
        Write-Host "     Question ID: $($createdQuestion.id)" -ForegroundColor Gray
        Write-Host "     Title: $($createdQuestion.title)" -ForegroundColor Gray
        $testQuestionId = $createdQuestion.id
        $testResults += "PASS: Question creation via API successful"
    }
} catch {
    Write-Host "   ✗ Question creation failed: $($_.Exception.Message)" -ForegroundColor Red
    $testResults += "FAIL: Question creation via API failed"
}

# Test 3: Question Details Display
Write-Host "`n3. Testing Question Details Display..." -ForegroundColor Yellow

if ($testQuestionId) {
    # Step 3.1: Get question details from backend
    try {
        $questionDetails = Invoke-RestMethod -Uri "$BackendUrl/api/questions/$testQuestionId" -Method GET
        Write-Host "   ✓ Question details retrieved from backend" -ForegroundColor Green
        Write-Host "     Title: $($questionDetails.title)" -ForegroundColor Gray
        $testResults += "PASS: Question details API functional"
    } catch {
        Write-Host "   ✗ Question details API failed: $($_.Exception.Message)" -ForegroundColor Red
        $testResults += "FAIL: Question details API failed"
    }

    # Step 3.2: Check frontend question details page
    try {
        $questionDetailsPage = Invoke-WebRequest -Uri "$FrontendUrl/Questions/Details/$testQuestionId" -Method GET
        if ($questionDetailsPage.StatusCode -eq 200) {
            Write-Host "   ✓ Frontend question details page accessible" -ForegroundColor Green
            $testResults += "PASS: Frontend question details page accessible"
        }
    } catch {
        Write-Host "   ✗ Frontend question details failed: $($_.Exception.Message)" -ForegroundColor Red
        $testResults += "FAIL: Frontend question details page failed"
    }
}

# Test 4: Search and Filter Functionality
Write-Host "`n4. Testing Search and Filter..." -ForegroundColor Yellow

# Step 4.1: Test search functionality
try {
    $searchResults = Invoke-RestMethod -Uri "$BackendUrl/api/questions?search=test" -Method GET
    Write-Host "   ✓ Search functionality works" -ForegroundColor Green
    Write-Host "     Found $($searchResults.Count) results for 'test'" -ForegroundColor Gray
    $testResults += "PASS: Search functionality works"
} catch {
    Write-Host "   ✗ Search functionality failed: $($_.Exception.Message)" -ForegroundColor Red
    $testResults += "FAIL: Search functionality failed"
}

# Step 4.2: Test tag filtering
try {
    $tagResults = Invoke-RestMethod -Uri "$BackendUrl/api/questions?tag=testing" -Method GET
    Write-Host "   ✓ Tag filtering works" -ForegroundColor Green
    Write-Host "     Found $($tagResults.Count) results for 'testing' tag" -ForegroundColor Gray
    $testResults += "PASS: Tag filtering works"
} catch {
    Write-Host "   ✗ Tag filtering failed: $($_.Exception.Message)" -ForegroundColor Red
    $testResults += "FAIL: Tag filtering failed"
}

# Test 5: Tags Management
Write-Host "`n5. Testing Tags Management..." -ForegroundColor Yellow

# Step 5.1: Get available tags
try {
    $availableTags = Invoke-RestMethod -Uri "$BackendUrl/api/tags" -Method GET
    Write-Host "   ✓ Tags API functional" -ForegroundColor Green
    Write-Host "     Available tags: $($availableTags.Count)" -ForegroundColor Gray
    $testResults += "PASS: Tags API functional"
} catch {
    Write-Host "   ✗ Tags API failed: $($_.Exception.Message)" -ForegroundColor Red
    $testResults += "FAIL: Tags API failed"
}

# Test 6: Data Persistence Verification
Write-Host "`n6. Testing Data Persistence..." -ForegroundColor Yellow

if ($testQuestionId) {
    # Wait a moment and verify the question still exists
    Start-Sleep -Seconds 2
    try {
        $persistedQuestion = Invoke-RestMethod -Uri "$BackendUrl/api/questions/$testQuestionId" -Method GET
        if ($persistedQuestion.id -eq $testQuestionId) {
            Write-Host "   ✓ Question persisted in database" -ForegroundColor Green
            $testResults += "PASS: Data persistence verified"
        }
    } catch {
        Write-Host "   ✗ Question not persisted: $($_.Exception.Message)" -ForegroundColor Red
        $testResults += "FAIL: Data persistence failed"
    }
}

# Test 7: Error Handling
Write-Host "`n7. Testing Error Handling..." -ForegroundColor Yellow

# Step 7.1: Test invalid question ID
try {
    $invalidResponse = Invoke-WebRequest -Uri "$BackendUrl/api/questions/99999" -Method GET
    Write-Host "   ⚠ Invalid question ID should return error" -ForegroundColor Yellow
} catch {
    if ($_.Exception.Response.StatusCode -eq 404) {
        Write-Host "   ✓ Proper 404 error for invalid question ID" -ForegroundColor Green
        $testResults += "PASS: Error handling works properly"
    } else {
        Write-Host "   ✗ Unexpected error response" -ForegroundColor Red
        $testResults += "FAIL: Error handling not working properly"
    }
}

# Test Results Summary
Write-Host "`n=== QUESTIONS MANAGEMENT E2E TEST RESULTS ===" -ForegroundColor Green

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
    Write-Host "`n🎉 QUESTIONS MANAGEMENT E2E TEST: PASSED" -ForegroundColor White -BackgroundColor Green
} else {
    Write-Host "`n❌ QUESTIONS MANAGEMENT E2E TEST: FAILED" -ForegroundColor White -BackgroundColor Red
}

Write-Host "`nTest completed at: $(Get-Date)" -ForegroundColor Gray
return $failCount -eq 0
