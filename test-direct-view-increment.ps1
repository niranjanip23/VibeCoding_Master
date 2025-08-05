# Direct SQL Test for View Count
Write-Host "=== Testing View Count at Database Level ===" -ForegroundColor Green

# Test SQL directly with curl to see what happens
Write-Host "Step 1: Check current view count for question 31..." -ForegroundColor Yellow
$initialResponse = Invoke-RestMethod -Uri "http://localhost:5031/api/questions/31" -Method GET
Write-Host "Initial Views: $($initialResponse.views)" -ForegroundColor Cyan

# Do it again to see if it increments
Write-Host ""
Write-Host "Step 2: Access question 31 again to trigger increment..." -ForegroundColor Yellow
$secondResponse = Invoke-RestMethod -Uri "http://localhost:5031/api/questions/31" -Method GET
Write-Host "Views after 2nd access: $($secondResponse.views)" -ForegroundColor Cyan

# One more time
Write-Host ""
Write-Host "Step 3: Access question 31 third time..." -ForegroundColor Yellow
$thirdResponse = Invoke-RestMethod -Uri "http://localhost:5031/api/questions/31" -Method GET
Write-Host "Views after 3rd access: $($thirdResponse.views)" -ForegroundColor Cyan

# Summary
Write-Host ""
Write-Host "=== RESULTS ===" -ForegroundColor Green
Write-Host "1st Access: $($initialResponse.views) views" -ForegroundColor White
Write-Host "2nd Access: $($secondResponse.views) views" -ForegroundColor White
Write-Host "3rd Access: $($thirdResponse.views) views" -ForegroundColor White

if ($thirdResponse.views -gt $initialResponse.views) {
    Write-Host "✅ View count is incrementing!" -ForegroundColor Green
} else {
    Write-Host "❌ View count is NOT incrementing!" -ForegroundColor Red
    Write-Host "Possible issues:" -ForegroundColor Yellow
    Write-Host "- Database column issue" -ForegroundColor Gray
    Write-Host "- SQL query not executing" -ForegroundColor Gray
    Write-Host "- Transaction rollback" -ForegroundColor Gray
    Write-Host "- Wrong field being updated/read" -ForegroundColor Gray
}
