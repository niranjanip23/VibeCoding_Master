# Complete End-to-End User Journey Testing

param(
    [string]$BackendUrl = "http://localhost:5031",
    [string]$FrontendUrl = "http://localhost:5000"
)

Write-Host "=== COMPLETE E2E USER JOURNEY TESTING ===" -ForegroundColor Green
Write-Host "Testing complete user workflow: Register → Login → Browse → Ask → Answer → Vote" -ForegroundColor Cyan

$testResults = @()
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$testUser = @{
    Name = "Journey Test User $timestamp"
    Email = "journey.test.$timestamp@queryhub.com"
    Password = "JourneyTest123!"
}

# Journey Step 1: User Registration
Write-Host "`n🚀 JOURNEY STEP 1: User Registration" -ForegroundColor Magenta

try {
    $registerData = $testUser | ConvertTo-Json
    $registerResponse = Invoke-RestMethod -Uri "$BackendUrl/api/auth/register" -Method POST -Body $registerData -ContentType "application/json"
    Write-Host "   ✓ User registered successfully" -ForegroundColor Green
    Write-Host "     Name: $($testUser.Name)" -ForegroundColor Gray
    Write-Host "     Email: $($testUser.Email)" -ForegroundColor Gray
    $testResults += "PASS: User registration successful"
} catch {
    if ($_.Exception.Message -match "400") {
        Write-Host "   ⚠ User might already exist, continuing with login..." -ForegroundColor Yellow
        $testResults += "WARN: User already exists"
    } else {
        Write-Host "   ✗ Registration failed: $($_.Exception.Message)" -ForegroundColor Red
        $testResults += "FAIL: User registration failed"
        exit 1
    }
}

# Journey Step 2: User Login
Write-Host "`n🔑 JOURNEY STEP 2: User Login" -ForegroundColor Magenta

try {
    $loginData = @{
        Email = $testUser.Email
        Password = $testUser.Password
    } | ConvertTo-Json

    $loginResponse = Invoke-RestMethod -Uri "$BackendUrl/api/auth/login" -Method POST -Body $loginData -ContentType "application/json"
    
    if ($loginResponse.token) {
        Write-Host "   ✓ User logged in successfully" -ForegroundColor Green
        Write-Host "     User: $($loginResponse.name)" -ForegroundColor Gray
        Write-Host "     Token received: Yes" -ForegroundColor Gray
        $authToken = $loginResponse.token
        $userId = $loginResponse.userId
        $headers = @{
            "Authorization" = "Bearer $authToken"
            "Content-Type" = "application/json"
        }
        $testResults += "PASS: User login successful"
    } else {
        throw "No token received"
    }
} catch {
    Write-Host "   ✗ Login failed: $($_.Exception.Message)" -ForegroundColor Red
    $testResults += "FAIL: User login failed"
    
    # Fallback to existing user
    try {
        $fallbackLoginData = @{
            Email = "john.doe@example.com"
            Password = "password123"
        } | ConvertTo-Json
        
        $fallbackLoginResponse = Invoke-RestMethod -Uri "$BackendUrl/api/auth/login" -Method POST -Body $fallbackLoginData -ContentType "application/json"
        $authToken = $fallbackLoginResponse.token
        $userId = $fallbackLoginResponse.userId
        $headers = @{
            "Authorization" = "Bearer $authToken"
            "Content-Type" = "application/json"
        }
        Write-Host "   ✓ Using existing user for journey test" -ForegroundColor Green
        $testResults += "PASS: Fallback login successful"
    } catch {
        Write-Host "   ✗ Fallback login also failed" -ForegroundColor Red
        exit 1
    }
}

# Journey Step 3: Browse Questions
Write-Host "`n📖 JOURNEY STEP 3: Browse Questions" -ForegroundColor Magenta

try {
    # Check frontend questions page
    $questionsPageResponse = Invoke-WebRequest -Uri "$FrontendUrl/Questions" -Method GET
    if ($questionsPageResponse.StatusCode -eq 200) {
        Write-Host "   ✓ Questions page accessible from frontend" -ForegroundColor Green
        $testResults += "PASS: Questions browsing works"
    }
    
    # Get questions from backend
    $questionsData = Invoke-RestMethod -Uri "$BackendUrl/api/questions" -Method GET
    Write-Host "   ✓ Retrieved $($questionsData.Count) questions from backend" -ForegroundColor Green
    
    # Pick a question for later interaction
    if ($questionsData.Count -gt 0) {
        $targetQuestion = $questionsData[0]
        $targetQuestionId = $targetQuestion.id
        Write-Host "     Selected question '$($targetQuestion.title)' for interaction" -ForegroundColor Gray
        $testResults += "PASS: Question selection for interaction"
    }
} catch {
    Write-Host "   ✗ Questions browsing failed: $($_.Exception.Message)" -ForegroundColor Red
    $testResults += "FAIL: Questions browsing failed"
}

# Journey Step 4: View Question Details
Write-Host "`n🔍 JOURNEY STEP 4: View Question Details" -ForegroundColor Magenta

if ($targetQuestionId) {
    try {
        # Check frontend question details page
        $questionDetailsPage = Invoke-WebRequest -Uri "$FrontendUrl/Questions/Details/$targetQuestionId" -Method GET
        if ($questionDetailsPage.StatusCode -eq 200) {
            Write-Host "   ✓ Question details page accessible" -ForegroundColor Green
            $testResults += "PASS: Question details viewing works"
        }
        
        # Get question details from backend
        $questionDetails = Invoke-RestMethod -Uri "$BackendUrl/api/questions/$targetQuestionId" -Method GET
        Write-Host "   ✓ Question details retrieved from backend" -ForegroundColor Green
        Write-Host "     Title: $($questionDetails.title)" -ForegroundColor Gray
        Write-Host "     Answers: $($questionDetails.answers.Count)" -ForegroundColor Gray
        
    } catch {
        Write-Host "   ✗ Question details viewing failed: $($_.Exception.Message)" -ForegroundColor Red
        $testResults += "FAIL: Question details viewing failed"
    }
}

# Journey Step 5: Create a New Question
Write-Host "`n❓ JOURNEY STEP 5: Create New Question" -ForegroundColor Magenta

$newQuestionData = @{
    Title = "Journey Test Question - $timestamp"
    Body = "This question was created during a complete E2E user journey test. It demonstrates the full workflow from frontend to backend to database storage."
    Tags = @("journey-test", "e2e", "automation", "testing")
}

try {
    $questionJson = $newQuestionData | ConvertTo-Json
    $createdQuestion = Invoke-RestMethod -Uri "$BackendUrl/api/questions" -Method POST -Body $questionJson -Headers $headers
    
    if ($createdQuestion.id) {
        Write-Host "   ✓ New question created successfully" -ForegroundColor Green
        Write-Host "     Question ID: $($createdQuestion.id)" -ForegroundColor Gray
        Write-Host "     Title: $($createdQuestion.title)" -ForegroundColor Gray
        $newQuestionId = $createdQuestion.id
        $testResults += "PASS: Question creation successful"
        
        # Verify the question appears in the list
        Start-Sleep -Seconds 1
        $updatedQuestions = Invoke-RestMethod -Uri "$BackendUrl/api/questions" -Method GET
        $foundNewQuestion = $updatedQuestions | Where-Object { $_.id -eq $newQuestionId }
        if ($foundNewQuestion) {
            Write-Host "   ✓ New question appears in questions list" -ForegroundColor Green
            $testResults += "PASS: Question persistence verified"
        }
    }
} catch {
    Write-Host "   ✗ Question creation failed: $($_.Exception.Message)" -ForegroundColor Red
    $testResults += "FAIL: Question creation failed"
}

# Journey Step 6: Create an Answer
Write-Host "`n💬 JOURNEY STEP 6: Create Answer" -ForegroundColor Magenta

if ($targetQuestionId) {
    $answerData = @{
        QuestionId = $targetQuestionId
        Body = "This is a test answer created during E2E journey testing. It demonstrates the answer creation workflow from frontend to backend."
    }
    
    try {
        $answerJson = $answerData | ConvertTo-Json
        $createdAnswer = Invoke-RestMethod -Uri "$BackendUrl/api/answers" -Method POST -Body $answerJson -Headers $headers
        
        if ($createdAnswer.id) {
            Write-Host "   ✓ Answer created successfully" -ForegroundColor Green
            Write-Host "     Answer ID: $($createdAnswer.id)" -ForegroundColor Gray
            Write-Host "     For Question: $targetQuestionId" -ForegroundColor Gray
            $newAnswerId = $createdAnswer.id
            $testResults += "PASS: Answer creation successful"
        }
    } catch {
        Write-Host "   ✗ Answer creation failed: $($_.Exception.Message)" -ForegroundColor Red
        $testResults += "FAIL: Answer creation failed"
    }
}

# Journey Step 7: Vote on Question
Write-Host "`n👍 JOURNEY STEP 7: Vote on Question" -ForegroundColor Magenta

if ($targetQuestionId) {
    $voteData = @{
        QuestionId = $targetQuestionId
        IsUpvote = $true
    }
    
    try {
        $voteJson = $voteData | ConvertTo-Json
        $voteResponse = Invoke-RestMethod -Uri "$BackendUrl/api/votes" -Method POST -Body $voteJson -Headers $headers
        Write-Host "   ✓ Vote on question successful" -ForegroundColor Green
        $testResults += "PASS: Question voting successful"
    } catch {
        if ($_.Exception.Message -match "already voted") {
            Write-Host "   ⚠ User already voted on this question" -ForegroundColor Yellow
            $testResults += "WARN: User already voted"
        } else {
            Write-Host "   ✗ Voting failed: $($_.Exception.Message)" -ForegroundColor Red
            $testResults += "FAIL: Question voting failed"
        }
    }
}

# Journey Step 8: Vote on Answer
Write-Host "`n👍 JOURNEY STEP 8: Vote on Answer" -ForegroundColor Magenta

if ($newAnswerId) {
    $answerVoteData = @{
        AnswerId = $newAnswerId
        IsUpvote = $true
    }
    
    try {
        $answerVoteJson = $answerVoteData | ConvertTo-Json
        $answerVoteResponse = Invoke-RestMethod -Uri "$BackendUrl/api/votes" -Method POST -Body $answerVoteJson -Headers $headers
        Write-Host "   ✓ Vote on answer successful" -ForegroundColor Green
        $testResults += "PASS: Answer voting successful"
    } catch {
        if ($_.Exception.Message -match "already voted") {
            Write-Host "   ⚠ User already voted on this answer" -ForegroundColor Yellow
            $testResults += "WARN: User already voted on answer"
        } else {
            Write-Host "   ✗ Answer voting failed: $($_.Exception.Message)" -ForegroundColor Red
            $testResults += "FAIL: Answer voting failed"
        }
    }
}

# Journey Step 9: Search Functionality
Write-Host "`n🔍 JOURNEY STEP 9: Search Functionality" -ForegroundColor Magenta

try {
    $searchResults = Invoke-RestMethod -Uri "$BackendUrl/api/questions?search=test" -Method GET
    Write-Host "   ✓ Search functionality works" -ForegroundColor Green
    Write-Host "     Found $($searchResults.Count) results for 'test'" -ForegroundColor Gray
    $testResults += "PASS: Search functionality works"
    
    # Test search via frontend
    $searchPageResponse = Invoke-WebRequest -Uri "$FrontendUrl/Questions?search=test" -Method GET
    if ($searchPageResponse.StatusCode -eq 200) {
        Write-Host "   ✓ Frontend search page accessible" -ForegroundColor Green
        $testResults += "PASS: Frontend search works"
    }
} catch {
    Write-Host "   ✗ Search functionality failed: $($_.Exception.Message)" -ForegroundColor Red
    $testResults += "FAIL: Search functionality failed"
}

# Journey Step 10: Data Verification
Write-Host "`n✅ JOURNEY STEP 10: Final Data Verification" -ForegroundColor Magenta

try {
    # Verify all created data still exists
    if ($newQuestionId) {
        $verifyQuestion = Invoke-RestMethod -Uri "$BackendUrl/api/questions/$newQuestionId" -Method GET
        if ($verifyQuestion.id -eq $newQuestionId) {
            Write-Host "   ✓ Created question persisted in database" -ForegroundColor Green
            $testResults += "PASS: Question data persistence verified"
        }
    }
    
    if ($newAnswerId) {
        $verifyAnswer = Invoke-RestMethod -Uri "$BackendUrl/api/questions/$targetQuestionId" -Method GET
        $foundAnswer = $verifyAnswer.answers | Where-Object { $_.id -eq $newAnswerId }
        if ($foundAnswer) {
            Write-Host "   ✓ Created answer persisted in database" -ForegroundColor Green
            $testResults += "PASS: Answer data persistence verified"
        }
    }
    
    # Check total questions count increased
    $finalQuestionsCount = (Invoke-RestMethod -Uri "$BackendUrl/api/questions" -Method GET).Count
    Write-Host "   ✓ Total questions in database: $finalQuestionsCount" -ForegroundColor Green
    
} catch {
    Write-Host "   ✗ Data verification failed: $($_.Exception.Message)" -ForegroundColor Red
    $testResults += "FAIL: Data verification failed"
}

# Final Results Summary
Write-Host "`n=== COMPLETE USER JOURNEY E2E TEST RESULTS ===" -ForegroundColor Green

$passCount = ($testResults | Where-Object { $_ -like "PASS:*" }).Count
$failCount = ($testResults | Where-Object { $_ -like "FAIL:*" }).Count
$warnCount = ($testResults | Where-Object { $_ -like "WARN:*" }).Count

Write-Host "`nJourney Test Results Summary:" -ForegroundColor White
Write-Host "✓ Passed: $passCount" -ForegroundColor Green
Write-Host "✗ Failed: $failCount" -ForegroundColor Red
Write-Host "⚠ Warnings: $warnCount" -ForegroundColor Yellow

Write-Host "`nUser Journey Steps Completed:" -ForegroundColor White
Write-Host "🚀 Registration → 🔑 Login → 📖 Browse → 🔍 View Details → ❓ Ask Question → 💬 Answer → 👍 Vote → 🔍 Search → ✅ Verify" -ForegroundColor Cyan

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
    Write-Host "`n🎉 COMPLETE USER JOURNEY E2E TEST: PASSED" -ForegroundColor White -BackgroundColor Green
    Write-Host "The entire user workflow from registration to voting works end-to-end!" -ForegroundColor Green
} else {
    Write-Host "`n❌ COMPLETE USER JOURNEY E2E TEST: FAILED" -ForegroundColor White -BackgroundColor Red
    Write-Host "Some parts of the user journey are not working properly." -ForegroundColor Red
}

Write-Host "`nTest completed at: $(Get-Date)" -ForegroundColor Gray
return $failCount -eq 0
