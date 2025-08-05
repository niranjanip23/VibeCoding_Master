# Simple Self-View Prevention Test
Write-Host "=== Testing Self-View Prevention ===" -ForegroundColor Green

$backendUrl = "http://localhost:5031"

# Test anonymous view first
try {
    $response = Invoke-RestMethod -Uri "$backendUrl/api/questions" -Method GET
    Write-Host "✓ Backend is accessible" -ForegroundColor Green
    Write-Host "Questions found: $($response.Count)" -ForegroundColor Cyan
    
    if ($response.Count -gt 0) {
        $testQuestion = $response[0]
        $questionId = $testQuestion.id
        $initialViews = $testQuestion.views
        
        Write-Host "`nTesting with question ID: $questionId" -ForegroundColor Yellow
        Write-Host "Initial view count: $initialViews" -ForegroundColor Cyan
        
        # Anonymous view (should increment)
        $questionAfterView = Invoke-RestMethod -Uri "$backendUrl/api/questions/$questionId" -Method GET
        $viewsAfterAnonymous = $questionAfterView.views
        
        Write-Host "View count after anonymous view: $viewsAfterAnonymous" -ForegroundColor Cyan
        
        if ($viewsAfterAnonymous -eq ($initialViews + 1)) {
            Write-Host "✓ Anonymous view correctly incremented view count" -ForegroundColor Green
        } else {
            Write-Host "✗ Anonymous view did not increment view count as expected" -ForegroundColor Red
        }
    }
}
catch {
    Write-Host "✗ Error connecting to backend: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`nTest completed." -ForegroundColor Yellow
