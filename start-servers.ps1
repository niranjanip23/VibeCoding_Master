# Start backend
Write-Host "Starting backend..." -ForegroundColor Green
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd 'QueryHub-Backend'; dotnet run" -WindowStyle Normal

# Wait a few seconds
Start-Sleep -Seconds 5

# Start frontend  
Write-Host "Starting frontend..." -ForegroundColor Green
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd 'QueryHub-Frontend'; dotnet run" -WindowStyle Normal

Write-Host "Both servers should be starting..." -ForegroundColor Cyan
Write-Host "Backend will be available at: http://localhost:5031" -ForegroundColor Yellow
Write-Host "Frontend will be available at: http://localhost:5000" -ForegroundColor Yellow
