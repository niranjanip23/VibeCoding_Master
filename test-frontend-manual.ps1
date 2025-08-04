# Test frontend registration and question creation workflow
$frontendUrl = "http://localhost:5000"

Write-Host "=== Testing Frontend UI Workflow ===" -ForegroundColor Green

# We'll simulate the frontend workflow
Write-Host "`n1. Frontend is running at: $frontendUrl" -ForegroundColor Yellow
Write-Host "2. Please manually:" -ForegroundColor Yellow
Write-Host "   a) Go to $frontendUrl/Account/Register" -ForegroundColor Cyan
Write-Host "   b) Register a new user" -ForegroundColor Cyan
Write-Host "   c) Go to $frontendUrl/Questions/Ask" -ForegroundColor Cyan
Write-Host "   d) Create a test question" -ForegroundColor Cyan
Write-Host "   e) Monitor the logs in this terminal" -ForegroundColor Cyan

Write-Host "`n3. After you test, check the logs below for detailed error information" -ForegroundColor Yellow

# Let's also check the current frontend logs
Write-Host "`nPress Enter when you're ready to check the latest frontend logs..."
Read-Host

Write-Host "`n=== End of Test Instructions ===" -ForegroundColor Green
