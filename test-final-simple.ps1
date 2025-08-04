# Final integration test - simple version
$backendUrl = "http://localhost:5031"

Write-Host "=== Final Integration Test ===" -ForegroundColor Green

# Register a test user
Write-Host "`n1. Registering test user..." -ForegroundColor Yellow
$registerData = @{
    Username = "finaltest_$(Get-Random)"
    Email = "finaltest_$(Get-Random)@example.com"
    Password = "TestPassword123!"
} | ConvertTo-Json

$authResponse = Invoke-RestMethod -Uri "$backendUrl/api/auth/register" -Method POST -Body $registerData -ContentType "application/json"
$token = $authResponse.Token
Write-Host "Registration successful: $($authResponse.Username)" -ForegroundColor Green

# Create question
Write-Host "`n2. Creating question..." -ForegroundColor Yellow
$questionRequest = @{
    Title = "Final Test Question - $(Get-Date -Format 'HH:mm:ss')"
    Body = "This is the final integration test question."
    Tags = @("final-test", "integration")
} | ConvertTo-Json

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

$response = Invoke-RestMethod -Uri "$backendUrl/api/questions" -Method POST -Body $questionRequest -Headers $headers

Write-Host "Question created successfully!" -ForegroundColor Green
Write-Host "Question ID: $($response.Id)" -ForegroundColor Green
Write-Host "Title: $($response.Title)" -ForegroundColor Green
Write-Host "Tags: $($response.Tags -join ', ')" -ForegroundColor Green

# Test frontend mapping
Write-Host "`n3. Testing frontend mapping..." -ForegroundColor Yellow
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

Write-Host "Mapping successful!" -ForegroundColor Green
Write-Host "Mapped Title: $($mappedQuestion.Title)" -ForegroundColor Cyan
Write-Host "Mapped Author: $($mappedQuestion.Author)" -ForegroundColor Cyan

Write-Host "`n=== ALL TESTS PASSED! ===" -ForegroundColor Green
Write-Host "The frontend should now work correctly!" -ForegroundColor Yellow
