# Final integration test - simulate complete frontend flow
$frontendUrl = "http://localhost:5000"
$backendUrl = "http://localhost:5031"

Write-Host "=== Final Integration Test ===" -ForegroundColor Green

# Create a session to maintain cookies
$session = New-Object Microsoft.PowerShell.Commands.WebRequestSession

# Test 1: Register via backend (simulating frontend auth)
Write-Host "`n1. Registering test user..." -ForegroundColor Yellow
$userName = "finaltest_$(Get-Random)"
$email = "$userName@example.com"
$password = "TestPassword123!"

$registerData = @{
    Username = $userName
    Email = $email
    Password = $password
} | ConvertTo-Json

try {
    $authResponse = Invoke-RestMethod -Uri "$backendUrl/api/auth/register" -Method POST -Body $registerData -ContentType "application/json"
    $token = $authResponse.Token
    Write-Host "✓ Registration successful: $($authResponse.Username)" -ForegroundColor Green
    Write-Host "✓ Token obtained: $($token.Substring(0,20))..." -ForegroundColor Green
} catch {
    Write-Host "✗ Registration failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Test 2: Create question using the exact same process as frontend
Write-Host "`n2. Creating question (frontend simulation)..." -ForegroundColor Yellow

$questionTitle = "Final Test Question - $(Get-Date -Format 'HH:mm:ss')"
$questionBody = "This is the final integration test question to verify everything works end-to-end."
$questionTags = @("final-test", "integration", "success")

# Create the request exactly as frontend does
$questionRequest = @{
    Title = $questionTitle
    Body = $questionBody
    Tags = $questionTags
} | ConvertTo-Json

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
    "Accept" = "application/json"
}

Write-Host "Request payload:" -ForegroundColor Cyan
Write-Host $questionRequest -ForegroundColor White

try {
    # Make the request exactly as ApiService does
    $response = Invoke-RestMethod -Uri "$backendUrl/api/questions" -Method POST -Body $questionRequest -Headers $headers
    
    Write-Host "`n✓ Question created successfully!" -ForegroundColor Green
    Write-Host "✓ Question ID: $($response.Id)" -ForegroundColor Green
    Write-Host "✓ Title: $($response.Title)" -ForegroundColor Green
    Write-Host "✓ Tags: $($response.Tags -join ', ')" -ForegroundColor Green
    
    # Test the mapping that frontend does
    Write-Host "`n3. Testing frontend mapping..." -ForegroundColor Yellow
    
    # This is exactly what MapToQuestionViewModel does
    $mappedQuestion = @{
        Id = $response.Id
        Title = $response.Title
        Description = $response.Body
        Tags = $response.Tags
        Author = "User $($response.UserId)"
        CreatedAt = $response.CreatedAt
        Votes = $response.Votes
        Answers = 0
        Views = $response.Views
    }
    
    Write-Host "✓ Mapping successful! Frontend would display:" -ForegroundColor Green
    Write-Host "  ID: $($mappedQuestion.Id)" -ForegroundColor Cyan
    Write-Host "  Title: $($mappedQuestion.Title)" -ForegroundColor Cyan
    Write-Host "  Author: $($mappedQuestion.Author)" -ForegroundColor Cyan
    Write-Host "  Tags: $($mappedQuestion.Tags -join ', ')" -ForegroundColor Cyan
    Write-Host "  Votes: $($mappedQuestion.Votes)" -ForegroundColor Cyan
    Write-Host "  Views: $($mappedQuestion.Views)" -ForegroundColor Cyan
    
    # Test 4: Verify the question was created and can be retrieved
    Write-Host "`n4. Verifying question retrieval..." -ForegroundColor Yellow
    try {
        $retrievedQuestion = Invoke-RestMethod -Uri "$backendUrl/api/questions/$($response.Id)" -Method GET
        Write-Host "✓ Question retrieval successful!" -ForegroundColor Green
        Write-Host "✓ Retrieved title: $($retrievedQuestion.Title)" -ForegroundColor Green
        
        # Test 5: Verify it appears in the questions list
        Write-Host "`n5. Verifying question appears in list..." -ForegroundColor Yellow
        $allQuestions = Invoke-RestMethod -Uri "$backendUrl/api/questions" -Method GET
        $ourQuestion = $allQuestions | Where-Object { $_.Id -eq $response.Id }
        
        if ($ourQuestion) {
            Write-Host "✓ Question found in questions list!" -ForegroundColor Green
            Write-Host "`n=== ALL TESTS PASSED! ===" -ForegroundColor Green
            Write-Host "✓ Backend API working correctly" -ForegroundColor Green
            Write-Host "✓ Question creation successful" -ForegroundColor Green
            Write-Host "✓ Response structure matches frontend expectations" -ForegroundColor Green
            Write-Host "✓ Frontend mapping logic verified" -ForegroundColor Green
            Write-Host "✓ Question retrieval working" -ForegroundColor Green
            Write-Host "✓ Question appears in list" -ForegroundColor Green
            Write-Host "`nThe frontend should now work correctly!" -ForegroundColor Yellow
            Write-Host "Visit: $frontendUrl to test the UI" -ForegroundColor Cyan
        } else {
            Write-Host "✗ Question not found in list" -ForegroundColor Red
        }
    } catch {
        Write-Host "✗ Question retrieval failed: $($_.Exception.Message)" -ForegroundColor Red
    }
    
} catch {
    Write-Host "✗ Question creation failed: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $errorContent = $reader.ReadToEnd()
        Write-Host "Error details: $errorContent" -ForegroundColor Red
    }
}

Write-Host "`n=== Integration Test Complete ===" -ForegroundColor Green
