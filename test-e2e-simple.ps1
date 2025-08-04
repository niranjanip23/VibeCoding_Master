# Complete E2E test - Frontend Question Creation Simulation
$frontendUrl = "http://localhost:5000"
$backendUrl = "http://localhost:5031"

Write-Host "=== Complete E2E Test - Frontend Question Creation ===" -ForegroundColor Green

# First, let's register and get a token directly from backend (simulating what frontend should do)
Write-Host "`n1. Getting authentication token..." -ForegroundColor Yellow

$registerData = @{
    Username = "e2etest_$(Get-Random)"
    Email = "e2etest_$(Get-Random)@example.com"
    Password = "TestPassword123!"
} | ConvertTo-Json

try {
    $authResponse = Invoke-RestMethod -Uri "$backendUrl/api/auth/register" -Method POST -Body $registerData -ContentType "application/json"
    $token = $authResponse.Token
    Write-Host "Authentication successful, token obtained: $($token.Substring(0,20))..." -ForegroundColor Green
} catch {
    Write-Host "Auth failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Now simulate what the frontend ApiService.CreateQuestionAsync does
Write-Host "`n2. Testing frontend-style question creation..." -ForegroundColor Yellow

$questionData = @{
    Title = "E2E Test Question - $(Get-Date -Format 'HH:mm:ss')"
    Body = "This is an E2E test question from the frontend simulation."
    Tags = @("e2e", "testing", "frontend")
} | ConvertTo-Json

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
    "Accept" = "application/json"
}

Write-Host "Request data:" -ForegroundColor Cyan
Write-Host $questionData -ForegroundColor White

try {
    Write-Host "`nSending request to: $backendUrl/api/questions" -ForegroundColor Cyan
    $response = Invoke-WebRequest -Uri "$backendUrl/api/questions" -Method POST -Body $questionData -Headers $headers -UseBasicParsing
    
    Write-Host "`nResponse Status Code: $($response.StatusCode)" -ForegroundColor Green
    Write-Host "`nResponse Content:" -ForegroundColor Cyan
    $responseContent = $response.Content
    Write-Host $responseContent -ForegroundColor White
    
    # Try to parse the JSON response
    try {
        $parsedResponse = $responseContent | ConvertFrom-Json
        Write-Host "`nParsed Response Object:" -ForegroundColor Green
        $parsedResponse | ConvertTo-Json -Depth 3 | Write-Host
        
        # Check if it matches our expected QuestionApiModel structure
        Write-Host "`n3. Verifying response structure..." -ForegroundColor Yellow
        $requiredFields = @("Id", "Title", "Body", "UserId", "Views", "Votes", "CreatedAt", "UpdatedAt", "Tags")
        $missingFields = @()
        
        foreach ($field in $requiredFields) {
            if (-not $parsedResponse.PSObject.Properties[$field]) {
                $missingFields += $field
            }
        }
        
        if ($missingFields.Count -eq 0) {
            Write-Host "All required fields present in response!" -ForegroundColor Green
            Write-Host "Response structure matches QuestionApiModel" -ForegroundColor Green
            
            # Test if frontend mapping would work
            Write-Host "`n4. Testing frontend mapping simulation..." -ForegroundColor Yellow
            $mappedQuestion = @{
                Id = $parsedResponse.Id
                Title = $parsedResponse.Title
                Description = $parsedResponse.Body
                Tags = $parsedResponse.Tags
                Author = "User $($parsedResponse.UserId)"
                CreatedAt = $parsedResponse.CreatedAt
                Votes = $parsedResponse.Votes
                Answers = 0
                Views = $parsedResponse.Views
            }
            
            Write-Host "Frontend mapping would succeed:" -ForegroundColor Green
            $mappedQuestion | ConvertTo-Json -Depth 2 | Write-Host
            
        } else {
            Write-Host "Missing fields in response: $($missingFields -join ', ')" -ForegroundColor Red
        }
        
    } catch {
        Write-Host "Failed to parse JSON response: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "Raw response content: $responseContent" -ForegroundColor Yellow
    }
    
} catch {
    Write-Host "Request failed: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $errorResponse = $_.Exception.Response.GetResponseStream()
        $reader = New-Object System.IO.StreamReader($errorResponse)
        $errorContent = $reader.ReadToEnd()
        Write-Host "Error response: $errorContent" -ForegroundColor Red
    }
}

Write-Host "`n=== E2E Test Completed ===" -ForegroundColor Green
