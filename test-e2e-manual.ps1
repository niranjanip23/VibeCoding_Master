# Manual End-to-End Testing Checklist

Write-Host "=== MANUAL E2E TESTING CHECKLIST ===" -ForegroundColor Green
Write-Host "Follow this checklist to manually test the complete QueryHub application" -ForegroundColor Cyan

$checklist = @(
    @{
        Category = "🚀 Initial Setup"
        Tasks = @(
            "Verify backend is running on http://localhost:5031",
            "Verify frontend is running on http://localhost:5000",
            "Check Swagger UI is accessible at http://localhost:5031/swagger",
            "Verify database file exists (queryhub.db)"
        )
    },
    @{
        Category = "🔑 Authentication Flow"
        Tasks = @(
            "Navigate to http://localhost:5000",
            "Click 'Register' link",
            "Fill registration form with valid data",
            "Submit registration form",
            "Verify success message appears",
            "Navigate to login page",
            "Enter registered credentials",
            "Verify successful login and redirection"
        )
    },
    @{
        Category = "📖 Questions Browsing"
        Tasks = @(
            "Click 'Questions' in navigation",
            "Verify questions list displays",
            "Check question titles, authors, dates appear",
            "Verify vote counts and answer counts display",
            "Click on a question title",
            "Verify question details page loads",
            "Check question body, tags, and answers display"
        )
    },
    @{
        Category = "🔍 Search and Filter"
        Tasks = @(
            "Use search box on questions page",
            "Enter search term and submit",
            "Verify filtered results appear",
            "Click on a tag",
            "Verify questions filtered by tag",
            "Clear search and verify all questions return"
        )
    },
    @{
        Category = "❓ Question Creation"
        Tasks = @(
            "Ensure you're logged in",
            "Click 'Ask Question' or navigate to /Questions/Ask",
            "Fill in question title",
            "Fill in question description",
            "Add tags",
            "Submit the form",
            "Verify question is created and displayed",
            "Check question appears in questions list"
        )
    },
    @{
        Category = "💬 Answer Creation"
        Tasks = @(
            "Navigate to a question details page",
            "Scroll to answer section",
            "Fill in answer content",
            "Submit answer",
            "Verify answer appears on the page",
            "Check answer displays correct author and timestamp"
        )
    },
    @{
        Category = "👍 Voting System"
        Tasks = @(
            "On a question details page, click upvote on question",
            "Verify vote count increases",
            "Try voting on an answer",
            "Verify answer vote count updates",
            "Attempt to vote again (should prevent double voting)"
        )
    },
    @{
        Category = "🔒 Authentication Security"
        Tasks = @(
            "Log out from the application",
            "Try to access /Questions/Ask (should redirect to login)",
            "Try voting while logged out (should fail)",
            "Log back in and verify protected actions work"
        )
    },
    @{
        Category = "📱 User Interface"
        Tasks = @(
            "Check responsive design on different screen sizes",
            "Verify navigation menu works properly",
            "Check all buttons and links are functional",
            "Verify error messages display properly",
            "Check success messages appear correctly"
        )
    },
    @{
        Category = "🔄 Data Persistence"
        Tasks = @(
            "Create a question",
            "Close browser and reopen",
            "Navigate back to the application",
            "Verify your question still exists",
            "Check all data persisted correctly"
        )
    }
)

Write-Host "`n=== MANUAL TESTING INSTRUCTIONS ===" -ForegroundColor Yellow

foreach ($section in $checklist) {
    Write-Host "`n$($section.Category)" -ForegroundColor Magenta
    $taskNumber = 1
    foreach ($task in $section.Tasks) {
        Write-Host "   $taskNumber. $task" -ForegroundColor White
        $taskNumber++
    }
}

Write-Host "`n=== TESTING TIPS ===" -ForegroundColor Yellow
Write-Host "• Open browser developer tools (F12) to check for JavaScript errors" -ForegroundColor Cyan
Write-Host "• Monitor browser network tab to see API calls being made" -ForegroundColor Cyan
Write-Host "• Check browser console for any error messages" -ForegroundColor Cyan
Write-Host "• Test with different browsers (Chrome, Firefox, Edge)" -ForegroundColor Cyan
Write-Host "• Try both HTTP (localhost:5000) and HTTPS (localhost:5001) versions" -ForegroundColor Cyan

Write-Host "`n=== EXPECTED RESULTS ===" -ForegroundColor Yellow
Write-Host "✓ All pages should load without errors" -ForegroundColor Green
Write-Host "✓ All forms should submit successfully" -ForegroundColor Green
Write-Host "✓ Data should persist between sessions" -ForegroundColor Green
Write-Host "✓ Authentication should work properly" -ForegroundColor Green
Write-Host "✓ All CRUD operations should function" -ForegroundColor Green
Write-Host "✓ Search and filtering should work" -ForegroundColor Green
Write-Host "✓ No JavaScript errors in console" -ForegroundColor Green

Write-Host "`n=== TROUBLESHOOTING ===" -ForegroundColor Yellow
Write-Host "If any test fails:" -ForegroundColor Red
Write-Host "• Check backend is running (http://localhost:5031/api/tags should return JSON)" -ForegroundColor White
Write-Host "• Check frontend is running (http://localhost:5000 should show home page)" -ForegroundColor White
Write-Host "• Verify database file exists and has data" -ForegroundColor White
Write-Host "• Check browser console for JavaScript errors" -ForegroundColor White
Write-Host "• Review browser network tab for failed API calls" -ForegroundColor White
Write-Host "• Restart both backend and frontend if needed" -ForegroundColor White

Write-Host "`n📝 Create a test report documenting:" -ForegroundColor Cyan
Write-Host "• Which tests passed/failed" -ForegroundColor White
Write-Host "• Screenshots of successful operations" -ForegroundColor White
Write-Host "• Any error messages encountered" -ForegroundColor White
Write-Host "• Browser and environment details" -ForegroundColor White
Write-Host "• Recommendations for improvements" -ForegroundColor White

Write-Host "`nHappy Testing! 🚀" -ForegroundColor Green
