# QueryHub Server Startup Script
# This script starts both the backend and frontend servers

Write-Host "Starting QueryHub servers..." -ForegroundColor Green

# Start Backend Server
Write-Host "Starting Backend Server (port 5031)..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd 'QueryHub-Backend'; dotnet run"

# Wait a moment for backend to start
Start-Sleep -Seconds 3

# Start Frontend Server  
Write-Host "Starting Frontend Server (port 5000)..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd 'QueryHub-Frontend'; dotnet run"

Write-Host "Both servers are starting..." -ForegroundColor Green
Write-Host "Frontend: http://localhost:5000" -ForegroundColor Cyan
Write-Host "Backend: http://localhost:5031" -ForegroundColor Cyan
Write-Host "Wait a few seconds for both servers to fully start." -ForegroundColor Yellow
