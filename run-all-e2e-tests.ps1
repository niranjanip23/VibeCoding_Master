# Master E2E Test Runner - Executes All End-to-End Tests

param(
    [string]$BackendUrl = "http://localhost:5031",
    [string]$FrontendUrl = "http://localhost:5000",
    [switch]$SkipManual = $false
)

Write-Host "=== QUERYHUB E2E TESTING SUITE ===" -ForegroundColor Green -BackgroundColor Black
Write-Host "Comprehensive end-to-end testing for QueryHub Stack Overflow clone" -ForegroundColor Cyan
Write-Host "Testing frontend-backend integration, user flows, and data persistence" -ForegroundColor Cyan

$timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
Write-Host "`nTest Suite Started: $timestamp" -ForegroundColor Gray

# Pre-flight checks
Write-Host "`n🔍 PRE-FLIGHT CHECKS" -ForegroundColor Yellow

Write-Host "Checking if backend is running..." -NoNewline
try {
    $backendCheck = Invoke-RestMethod -Uri "$BackendUrl/api/tags" -Method GET -TimeoutSec 5
    Write-Host " ✓ Backend is running" -ForegroundColor Green
    Write-Host "   API URL: $BackendUrl" -ForegroundColor Gray
    Write-Host "   Database has $($backendCheck.Count) tags" -ForegroundColor Gray
} catch {
    Write-Host " ✗ Backend is not running!" -ForegroundColor Red
    Write-Host "Please start the backend first: cd QueryHub-Backend && dotnet run" -ForegroundColor Yellow
    exit 1
}

Write-Host "Checking if frontend is running..." -NoNewline
try {
    $frontendCheck = Invoke-WebRequest -Uri $FrontendUrl -Method GET -TimeoutSec 5
    Write-Host " ✓ Frontend is running" -ForegroundColor Green
    Write-Host "   Frontend URL: $FrontendUrl" -ForegroundColor Gray
} catch {
    Write-Host " ✗ Frontend is not running!" -ForegroundColor Red
    Write-Host "Please start the frontend first: cd QueryHub-Frontend && dotnet run" -ForegroundColor Yellow
    exit 1
}

$testResults = @()

# Test Suite 1: Authentication Flow
Write-Host "`n🔐 TEST SUITE 1: AUTHENTICATION FLOW" -ForegroundColor Yellow
Write-Host "Running authentication E2E tests..." -ForegroundColor Cyan

try {
    $authTestResult = & ".\test-e2e-auth.ps1" -BackendUrl $BackendUrl -FrontendUrl $FrontendUrl
    if ($authTestResult) {
        Write-Host "Authentication tests: PASSED ✓" -ForegroundColor Green
        $testResults += @{ Suite = "Authentication"; Result = "PASS" }
    } else {
        Write-Host "Authentication tests: FAILED ✗" -ForegroundColor Red
        $testResults += @{ Suite = "Authentication"; Result = "FAIL" }
    }
} catch {
    Write-Host "Authentication tests: ERROR - $($_.Exception.Message)" -ForegroundColor Red
    $testResults += @{ Suite = "Authentication"; Result = "ERROR" }
}

Write-Host "`nPress any key to continue to next test suite..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

# Test Suite 2: Questions Management
Write-Host "`n❓ TEST SUITE 2: QUESTIONS MANAGEMENT" -ForegroundColor Yellow
Write-Host "Running questions management E2E tests..." -ForegroundColor Cyan

try {
    $questionsTestResult = & ".\test-e2e-questions.ps1" -BackendUrl $BackendUrl -FrontendUrl $FrontendUrl
    if ($questionsTestResult) {
        Write-Host "Questions tests: PASSED ✓" -ForegroundColor Green
        $testResults += @{ Suite = "Questions"; Result = "PASS" }
    } else {
        Write-Host "Questions tests: FAILED ✗" -ForegroundColor Red
        $testResults += @{ Suite = "Questions"; Result = "FAIL" }
    }
} catch {
    Write-Host "Questions tests: ERROR - $($_.Exception.Message)" -ForegroundColor Red
    $testResults += @{ Suite = "Questions"; Result = "ERROR" }
}

Write-Host "`nPress any key to continue to complete journey test..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

# Test Suite 3: Complete User Journey
Write-Host "`n🚀 TEST SUITE 3: COMPLETE USER JOURNEY" -ForegroundColor Yellow
Write-Host "Running complete user journey E2E test..." -ForegroundColor Cyan

try {
    $journeyTestResult = & ".\test-e2e-complete.ps1" -BackendUrl $BackendUrl -FrontendUrl $FrontendUrl
    if ($journeyTestResult) {
        Write-Host "Complete journey test: PASSED ✓" -ForegroundColor Green
        $testResults += @{ Suite = "Complete Journey"; Result = "PASS" }
    } else {
        Write-Host "Complete journey test: FAILED ✗" -ForegroundColor Red
        $testResults += @{ Suite = "Complete Journey"; Result = "FAIL" }
    }
} catch {
    Write-Host "Complete journey test: ERROR - $($_.Exception.Message)" -ForegroundColor Red
    $testResults += @{ Suite = "Complete Journey"; Result = "ERROR" }
}

# Manual Testing Guide
if (-not $SkipManual) {
    Write-Host "`n📋 MANUAL TESTING GUIDE" -ForegroundColor Yellow
    Write-Host "Displaying manual testing checklist..." -ForegroundColor Cyan
    
    Write-Host "`nPress any key to view manual testing checklist..." -ForegroundColor Gray
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    
    try {
        & ".\test-e2e-manual.ps1"
        $testResults += @{ Suite = "Manual Guide"; Result = "DISPLAYED" }
    } catch {
        Write-Host "Manual testing guide: ERROR - $($_.Exception.Message)" -ForegroundColor Red
        $testResults += @{ Suite = "Manual Guide"; Result = "ERROR" }
    }
}

# Final Results Summary
Write-Host "`n" + "="*80 -ForegroundColor Green
Write-Host "QUERYHUB E2E TESTING SUITE - FINAL RESULTS" -ForegroundColor Green -BackgroundColor Black
Write-Host "="*80 -ForegroundColor Green

$passCount = ($testResults | Where-Object { $_.Result -eq "PASS" }).Count
$failCount = ($testResults | Where-Object { $_.Result -eq "FAIL" }).Count
$errorCount = ($testResults | Where-Object { $_.Result -eq "ERROR" }).Count

Write-Host "`nTest Suite Results:" -ForegroundColor White
foreach ($result in $testResults) {
    $icon = switch ($result.Result) {
        "PASS" { "✓"; "Green" }
        "FAIL" { "✗"; "Red" }
        "ERROR" { "⚠"; "Yellow" }
        "DISPLAYED" { "📋"; "Cyan" }
        default { "?"; "Gray" }
    }
    Write-Host "  $($icon[0]) $($result.Suite): $($result.Result)" -ForegroundColor $icon[1]
}

Write-Host "`nOverall Summary:" -ForegroundColor White
Write-Host "✓ Passed: $passCount" -ForegroundColor Green
Write-Host "✗ Failed: $failCount" -ForegroundColor Red
Write-Host "⚠ Errors: $errorCount" -ForegroundColor Yellow

if ($failCount -eq 0 -and $errorCount -eq 0) {
    Write-Host "`n🎉 ALL E2E TESTS PASSED!" -ForegroundColor White -BackgroundColor Green
    Write-Host "QueryHub frontend-backend integration is working perfectly!" -ForegroundColor Green
    Write-Host "`nRecommendations:" -ForegroundColor Cyan
    Write-Host "• The application is ready for production deployment" -ForegroundColor White
    Write-Host "• All user flows work end-to-end" -ForegroundColor White
    Write-Host "• Data persistence is working correctly" -ForegroundColor White
    Write-Host "• Authentication and authorization are functional" -ForegroundColor White
} elseif ($failCount -gt 0) {
    Write-Host "`n❌ SOME E2E TESTS FAILED" -ForegroundColor White -BackgroundColor Red
    Write-Host "Please review the failed tests and fix the issues before deployment." -ForegroundColor Red
    Write-Host "`nNext Steps:" -ForegroundColor Cyan
    Write-Host "• Review the detailed test output above" -ForegroundColor White
    Write-Host "• Fix the failing functionality" -ForegroundColor White
    Write-Host "• Re-run the test suite" -ForegroundColor White
    Write-Host "• Consider manual testing for additional verification" -ForegroundColor White
} else {
    Write-Host "`n⚠ E2E TESTS COMPLETED WITH ERRORS" -ForegroundColor White -BackgroundColor Yellow
    Write-Host "Some tests encountered errors. Please review and retry." -ForegroundColor Yellow
}

Write-Host "`nTesting Environment:" -ForegroundColor Gray
Write-Host "• Backend: $BackendUrl" -ForegroundColor Gray
Write-Host "• Frontend: $FrontendUrl" -ForegroundColor Gray
Write-Host "• Completed: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Gray

Write-Host "`nFor manual verification, visit:" -ForegroundColor Cyan
Write-Host "• Frontend: $FrontendUrl" -ForegroundColor White
Write-Host "• Backend API: $BackendUrl/swagger" -ForegroundColor White

Write-Host "`nE2E Testing Suite Completed! 🚀" -ForegroundColor Green

return ($failCount -eq 0 -and $errorCount -eq 0)
