# Test question details API endpoint
$backendUrl = "http://localhost:5031"

Write-Host "=== Testing Question Details API ===" -ForegroundColor Green

# Test getting a question that should exist (from our previous tests)
$questionId = 13  # From our earlier tests
Write-Host "`nTesting question details for ID: $questionId" -ForegroundColor Yellow

try {
    $response = Invoke-RestMethod -Uri "$backendUrl/api/questions/$questionId" -Method GET
    Write-Host "Question details retrieved successfully!" -ForegroundColor Green
    Write-Host "Full Response:" -ForegroundColor Cyan
    $response | ConvertTo-Json -Depth 3 | Write-Host
    
    # Check the structure
    Write-Host "`nStructure Analysis:" -ForegroundColor Yellow
    $response | Get-Member -MemberType Properties | ForEach-Object {
        Write-Host "  $($_.Name): $($_.Definition)" -ForegroundColor White
    }
    
} catch {
    Write-Host "Failed to get question details: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`nTest Complete" -ForegroundColor Green
