# Comprehensive Self-View Prevention Test for All Users
Write-Host "=== Testing Self-View Prevention for All Users ===" -ForegroundColor Green

$backendUrl = "http://localhost:5031"

# Test users
$users = @(
    @{ email = "john@queryhub.com"; password = "password123"; name = "John" },
    @{ email = "jane@queryhub.com"; password = "password123"; name = "Jane" },
    @{ email = "bob@queryhub.com"; password = "password123"; name = "Bob" }
)

# Function to login and get user info
function Get-UserInfo {
    param($email, $password, $name)
    
    try {
        $loginData = @{
            email = $email
            password = $password
        } | ConvertTo-Json
        
        $response = Invoke-RestMethod -Uri "$backendUrl/api/auth/login" -Method POST -Body $loginData -ContentType "application/json"
        return @{
            userId = $response.userId
            token = $response.token
            name = $name
            email = $email
        }
    }
    catch {
        Write-Host "Failed to login $name ($email): $($_.Exception.Message)" -ForegroundColor Red
        return $null
    }
}

# Get all user info
$userInfos = @()
foreach ($user in $users) {
    $info = Get-UserInfo -email $user.email -password $user.password -name $user.name
    if ($info) {
        $userInfos += $info
        Write-Host "✓ Logged in $($info.name) (ID: $($info.userId))" -ForegroundColor Green
    }
}

if ($userInfos.Count -eq 0) {
    Write-Host "No users could be logged in. Exiting." -ForegroundColor Red
    exit 1
}

# Get all questions
$questions = Invoke-RestMethod -Uri "$backendUrl/api/questions" -Method GET
Write-Host "`nFound $($questions.Count) questions in the system" -ForegroundColor Cyan

# Test each user viewing questions they created vs questions by others
foreach ($currentUser in $userInfos) {
    Write-Host "`n=== Testing for $($currentUser.name) (ID: $($currentUser.userId)) ===" -ForegroundColor Yellow
    
    $headers = @{ "Authorization" = "Bearer $($currentUser.token)" }
    
    # Find a question created by this user
    $ownQuestion = $questions | Where-Object { $_.userId -eq $currentUser.userId } | Select-Object -First 1
    
    # Find a question created by someone else
    $otherQuestion = $questions | Where-Object { $_.userId -ne $currentUser.userId } | Select-Object -First 1
    
    if ($ownQuestion) {
        Write-Host "`n1. Testing self-view for question $($ownQuestion.id) (created by user $($ownQuestion.userId))" -ForegroundColor Cyan
        
        # Get current view count
        $beforeViews = $ownQuestion.views
        Write-Host "   Views before self-view: $beforeViews"
        
        # View own question
        $afterView = Invoke-RestMethod -Uri "$backendUrl/api/questions/$($ownQuestion.id)" -Method GET -Headers $headers
        $afterViews = $afterView.views
        
        Write-Host "   Views after self-view: $afterViews"
        
        if ($afterViews -eq $beforeViews) {
            Write-Host "   ✓ PASS: Self-view did NOT increment view count" -ForegroundColor Green
        } else {
            Write-Host "   ✗ FAIL: Self-view incorrectly incremented view count" -ForegroundColor Red
        }
    } else {
        Write-Host "`n1. No questions found created by $($currentUser.name)" -ForegroundColor Gray
    }
    
    if ($otherQuestion) {
        Write-Host "`n2. Testing other-user-view for question $($otherQuestion.id) (created by user $($otherQuestion.userId))" -ForegroundColor Cyan
        
        # Get current view count
        $beforeViews = $otherQuestion.views
        Write-Host "   Views before other-user-view: $beforeViews"
        
        # View other user's question
        $afterView = Invoke-RestMethod -Uri "$backendUrl/api/questions/$($otherQuestion.id)" -Method GET -Headers $headers
        $afterViews = $afterView.views
        
        Write-Host "   Views after other-user-view: $afterViews"
        
        if ($afterViews -eq ($beforeViews + 1)) {
            Write-Host "   ✓ PASS: Other-user-view correctly incremented view count" -ForegroundColor Green
        } else {
            Write-Host "   ✗ FAIL: Other-user-view did not increment view count as expected" -ForegroundColor Red
        }
        
        # Update the question data for next iteration
        $questions | Where-Object { $_.id -eq $otherQuestion.id } | ForEach-Object { $_.views = $afterViews }
    } else {
        Write-Host "`n2. No questions found created by other users" -ForegroundColor Gray
    }
}

Write-Host "`n=== Testing Anonymous Views ===" -ForegroundColor Yellow

$testQuestion = $questions | Select-Object -First 1
if ($testQuestion) {
    Write-Host "`nTesting anonymous view for question $($testQuestion.id)" -ForegroundColor Cyan
    
    $beforeViews = $testQuestion.views
    Write-Host "Views before anonymous view: $beforeViews"
    
    $afterView = Invoke-RestMethod -Uri "$backendUrl/api/questions/$($testQuestion.id)" -Method GET
    $afterViews = $afterView.views
    
    Write-Host "Views after anonymous view: $afterViews"
    
    if ($afterViews -eq ($beforeViews + 1)) {
        Write-Host "✓ PASS: Anonymous view correctly incremented view count" -ForegroundColor Green
    } else {
        Write-Host "✗ FAIL: Anonymous view did not increment view count as expected" -ForegroundColor Red
    }
}

Write-Host "`n=== Test Completed ===" -ForegroundColor Green
