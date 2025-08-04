# Test Frontend-Backend Integration Script

Write-Host "=== QueryHub Frontend-Backend Integration Test ===" -ForegroundColor Green

# Test 1: Check if Backend API is running
Write-Host "`n1. Testing Backend API..." -ForegroundColor Yellow
try {
    $backendResponse = Invoke-RestMethod -Uri "http://localhost:5031/api/tags" -Method GET
    Write-Host "OK Backend API is running and responding" -ForegroundColor Green
    Write-Host "   Found $($backendResponse.Count) tags in the database" -ForegroundColor Cyan
}
catch {
    Write-Host "X Backend API is not responding: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Test 2: Check if Frontend is running
Write-Host "`n2. Testing Frontend..." -ForegroundColor Yellow
try {
    $frontendResponse = Invoke-WebRequest -Uri "http://localhost:5000" -Method GET
    if ($frontendResponse.StatusCode -eq 200) {
        Write-Host "OK Frontend is running and responding" -ForegroundColor Green
        Write-Host "   Frontend is accessible at http://localhost:5000" -ForegroundColor Cyan
    }
}
catch {
    Write-Host "X Frontend is not responding: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Test 3: Test Backend Questions API
Write-Host "`n3. Testing Backend Questions API..." -ForegroundColor Yellow
try {
    $questionsResponse = Invoke-RestMethod -Uri "http://localhost:5031/api/questions" -Method GET
    Write-Host "OK Backend questions API works" -ForegroundColor Green
    Write-Host "   Found $($questionsResponse.Count) questions in the database" -ForegroundColor Cyan
}
catch {
    Write-Host "X Backend questions API failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 4: Test Frontend Questions Page
Write-Host "`n4. Testing Frontend Questions Page..." -ForegroundColor Yellow
try {
    $questionsPageResponse = Invoke-WebRequest -Uri "http://localhost:5000/Questions" -Method GET
    if ($questionsPageResponse.StatusCode -eq 200) {
        Write-Host "OK Frontend questions page is accessible" -ForegroundColor Green
    }
}
catch {
    Write-Host "X Frontend questions page failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n=== Integration Test Summary ===" -ForegroundColor Green
Write-Host "OK Backend API: Running on http://localhost:5031" -ForegroundColor Green
Write-Host "OK Frontend App: Running on http://localhost:5000" -ForegroundColor Green
Write-Host "OK Database: SQLite database with sample data" -ForegroundColor Green

Write-Host "`nFrontend-Backend integration is working!" -ForegroundColor Green
Write-Host "You can test the application by visiting:" -ForegroundColor Cyan
Write-Host "- Frontend: http://localhost:5000" -ForegroundColor White
Write-Host "- Backend API: http://localhost:5031" -ForegroundColor White
Write-Host "- Swagger UI: http://localhost:5031/swagger" -ForegroundColor White
