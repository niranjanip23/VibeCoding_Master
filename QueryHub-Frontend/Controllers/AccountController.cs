using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using QueryHub_Frontend.Models;
using QueryHub_Frontend.Services;
using System.Security.Claims;

namespace QueryHub_Frontend.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IApiService _apiService;

        public AccountController(ILogger<AccountController> logger, IApiService apiService)
        {
            _logger = logger;
            _apiService = apiService;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _apiService.LoginAsync(model.Email, model.Password);
                    
                    if (result.Success && !string.IsNullOrEmpty(result.Token))
                    {
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, result.UserName ?? "User"),
                            new Claim(ClaimTypes.Email, model.Email),
                            new Claim("UserId", result.UserId ?? "0"),
                            new Claim("Token", result.Token)
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var authProperties = new AuthenticationProperties
                        {
                            IsPersistent = model.RememberMe,
                            ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(24)
                        };

                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity), authProperties);

                        TempData["SuccessMessage"] = "Welcome to QueryHub!";

                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, result.Message ?? "Invalid email or password.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during login");
                    ModelState.AddModelError(string.Empty, "An error occurred during login. Please try again.");
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["SuccessMessage"] = "You have been logged out successfully.";
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _apiService.RegisterAsync(model.Name, model.Username, model.Email, model.Password, model.Department);
                    
                    if (result.Success)
                    {
                        TempData["SuccessMessage"] = $"Welcome to QueryHub, {model.Name}! Your account has been created successfully. Please log in.";
                        return RedirectToAction("Login");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, result.Message ?? "Registration failed. Please try again.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during registration");
                    ModelState.AddModelError(string.Empty, "An error occurred during registration. Please try again.");
                }
            }

            return View(model);
        }

        public async Task<IActionResult> Profile()
        {
            if (!User.Identity!.IsAuthenticated)
            {
                return RedirectToAction("Login");
            }

            var currentUsername = User.Identity.Name ?? "Demo User";
            
            // Create user object with basic info
            var user = new User
            {
                Id = 1,
                Name = currentUsername,
                Email = User.FindFirst(ClaimTypes.Email)?.Value ?? "demo@queryhub.com",
                Department = User.FindFirst("Department")?.Value ?? "IT Department",
                Avatar = null,
                Reputation = 100,
                JoinedDate = DateTime.Now.AddMonths(-6)
            };

            // Initialize with safe default values
            ViewBag.QuestionsAsked = 0;
            ViewBag.AnswersGiven = 0;
            ViewBag.TotalVotes = 0;
            ViewBag.TotalViews = 0;
            ViewBag.RecentActivity = new List<object>();

            try
            {
                // Get all questions with full answer details
                var allQuestionsWithAnswers = await _apiService.GetQuestionsWithAnswersAsync("", "", 1, 1000);
                var userQuestions = allQuestionsWithAnswers.Where(q => q.Username.Equals(currentUsername, StringComparison.OrdinalIgnoreCase)).ToList();
                
                // Calculate questions asked
                ViewBag.QuestionsAsked = userQuestions.Count;
                ViewBag.TotalVotes = userQuestions.Sum(q => q.Votes);
                ViewBag.TotalViews = userQuestions.Sum(q => q.Views);

                // Calculate answers given by this user across ALL questions
                int answersGiven = 0;
                int totalAnswerVotes = 0;
                int acceptedAnswers = 0;
                var userAnswers = new List<dynamic>();
                
                foreach (var question in allQuestionsWithAnswers)
                {
                    if (question.Answers != null)
                    {
                        foreach (var answer in question.Answers)
                        {
                            if (!string.IsNullOrEmpty(answer.Username) && answer.Username.Equals(currentUsername, StringComparison.OrdinalIgnoreCase))
                            {
                                answersGiven++;
                                totalAnswerVotes += answer.Votes; // Changed from VoteCount to Votes
                                if (answer.IsAccepted) acceptedAnswers++;
                                
                                userAnswers.Add(new {
                                    Type = "answer",
                                    Title = question.Title,
                                    CreatedDate = answer.CreatedAt,
                                    Id = answer.Id,
                                    QuestionId = question.Id,
                                    Votes = answer.Votes, // Changed from VoteCount to Votes
                                    IsAccepted = answer.IsAccepted
                                });
                            }
                        }
                    }
                }
                
                ViewBag.AnswersGiven = answersGiven;
                
                // Dynamic Points Calculation (No minimum baseline - truly dynamic)
                int dynamicPoints = 0;
                dynamicPoints += ViewBag.TotalVotes * 5;      // 5 points per vote on questions
                dynamicPoints += ViewBag.QuestionsAsked * 10; // 10 points per question asked  
                dynamicPoints += answersGiven * 15;           // 15 points per answer given
                dynamicPoints += totalAnswerVotes * 10;       // 10 points per vote on answers
                dynamicPoints += acceptedAnswers * 25;        // 25 bonus points for accepted answers
                dynamicPoints += ViewBag.TotalViews * 1;      // 1 point per view on questions
                
                user.Reputation = dynamicPoints;
                
                // Store additional stats for display
                ViewBag.TotalAnswerVotes = totalAnswerVotes;
                ViewBag.AcceptedAnswers = acceptedAnswers;
                ViewBag.PointsBreakdown = new {
                    QuestionVotes = ViewBag.TotalVotes * 5,
                    QuestionsAsked = ViewBag.QuestionsAsked * 10,
                    AnswersGiven = answersGiven * 15,
                    AnswerVotes = totalAnswerVotes * 10,
                    AcceptedAnswers = acceptedAnswers * 25,
                    ViewsReceived = ViewBag.TotalViews * 1,
                    Total = dynamicPoints
                };

                // Create comprehensive recent activity combining questions and answers
                var recentActivity = new List<dynamic>();
                
                // Add user's recent questions
                foreach (var q in userQuestions.OrderByDescending(q => q.CreatedAt).Take(10))
                {
                    recentActivity.Add(new {
                        Type = "question",
                        Title = q.Title,
                        CreatedDate = q.CreatedAt,
                        Id = q.Id,
                        QuestionId = q.Id
                    });
                }
                
                // Add user's recent answers
                recentActivity.AddRange(userAnswers.OrderByDescending(a => a.CreatedDate).Take(10));
                
                // Sort all activity by date and take top 10 most recent
                ViewBag.RecentActivity = recentActivity.OrderByDescending(a => a.CreatedDate).Take(10).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading profile data for user: {Username}", currentUsername);
                // Keep default values
            }

            return View(user);
        }

        // Temporary test method to test profile without authentication
        public async Task<IActionResult> TestProfile(string username = "johndoe")
        {
            // Create user object with basic info
            var user = new User
            {
                Id = 1,
                Name = username,
                Email = $"{username}@queryhub.com",
                Department = "IT Department",
                Avatar = null,
                Reputation = 100,
                JoinedDate = DateTime.Now.AddMonths(-6)
            };

            // Initialize with safe default values
            ViewBag.QuestionsAsked = 0;
            ViewBag.AnswersGiven = 0;
            ViewBag.TotalVotes = 0;
            ViewBag.TotalViews = 0;
            ViewBag.RecentActivity = new List<object>();

            try
            {
                // Get all questions with full answer details
                var allQuestionsWithAnswers = await _apiService.GetQuestionsWithAnswersAsync("", "", 1, 1000);
                var userQuestions = allQuestionsWithAnswers.Where(q => q.Username.Equals(username, StringComparison.OrdinalIgnoreCase)).ToList();
                
                // Calculate questions asked
                ViewBag.QuestionsAsked = userQuestions.Count;
                ViewBag.TotalVotes = userQuestions.Sum(q => q.Votes);
                ViewBag.TotalViews = userQuestions.Sum(q => q.Views);

                // Calculate answers given by this user across ALL questions
                int answersGiven = 0;
                int totalAnswerVotes = 0;
                int acceptedAnswers = 0;
                var userAnswers = new List<dynamic>();
                
                foreach (var question in allQuestionsWithAnswers)
                {
                    if (question.Answers != null)
                    {
                        foreach (var answer in question.Answers)
                        {
                            if (!string.IsNullOrEmpty(answer.Username) && answer.Username.Equals(username, StringComparison.OrdinalIgnoreCase))
                            {
                                answersGiven++;
                                totalAnswerVotes += answer.Votes; // Changed from VoteCount to Votes
                                if (answer.IsAccepted) acceptedAnswers++;
                                
                                userAnswers.Add(new {
                                    Type = "answer",
                                    Title = question.Title,
                                    CreatedDate = answer.CreatedAt,
                                    Id = answer.Id,
                                    QuestionId = question.Id,
                                    Votes = answer.Votes, // Changed from VoteCount to Votes
                                    IsAccepted = answer.IsAccepted
                                });
                            }
                        }
                    }
                }
                
                ViewBag.AnswersGiven = answersGiven;
                
                // Dynamic Points Calculation (No minimum baseline - truly dynamic)
                int dynamicPoints = 0;
                dynamicPoints += ViewBag.TotalVotes * 5;      // 5 points per vote on questions
                dynamicPoints += ViewBag.QuestionsAsked * 10; // 10 points per question asked  
                dynamicPoints += answersGiven * 15;           // 15 points per answer given
                dynamicPoints += totalAnswerVotes * 10;       // 10 points per vote on answers
                dynamicPoints += acceptedAnswers * 25;        // 25 bonus points for accepted answers
                dynamicPoints += ViewBag.TotalViews * 1;      // 1 point per view on questions
                
                user.Reputation = dynamicPoints;
                
                // Store additional stats for display
                ViewBag.TotalAnswerVotes = totalAnswerVotes;
                ViewBag.AcceptedAnswers = acceptedAnswers;
                ViewBag.PointsBreakdown = new {
                    QuestionVotes = ViewBag.TotalVotes * 5,
                    QuestionsAsked = ViewBag.QuestionsAsked * 10,
                    AnswersGiven = answersGiven * 15,
                    AnswerVotes = totalAnswerVotes * 10,
                    AcceptedAnswers = acceptedAnswers * 25,
                    ViewsReceived = ViewBag.TotalViews * 1,
                    Total = dynamicPoints
                };
                
                // Add debug info
                ViewBag.DebugInfo = $"Total Questions: {allQuestionsWithAnswers.Count}, User Questions: {userQuestions.Count}, User Answers: {answersGiven}, Answer Votes: {totalAnswerVotes}, Accepted: {acceptedAnswers}, Dynamic Points: {dynamicPoints}";
            }
            catch (Exception ex)
            {
                ViewBag.DebugInfo = $"Error: {ex.Message}";
                // Keep default values
            }

            return View("Profile", user);
        }

        // Debug action to test HTTPS
        public IActionResult Test()
        {
            ViewBag.Protocol = Request.Scheme;
            ViewBag.Host = Request.Host;
            ViewBag.IsHttps = Request.IsHttps;
            return Content($"Protocol: {Request.Scheme}, Host: {Request.Host}, HTTPS: {Request.IsHttps}");
        }

        // Debug action to test authentication
        public IActionResult Debug()
        {
            return View();
        }

        // Simple login test
        public IActionResult SimpleTest()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SimpleLogin(string email, string password)
        {
            try
            {
                _logger.LogInformation("Testing simple login for {Email}", email);
                
                // Test direct API call
                var result = await _apiService.LoginAsync(email, password);
                
                TempData["LoginResult"] = $"Success: {result.Success}\nMessage: {result.Message}\nToken: {(result.Token?.Length > 20 ? result.Token[..20] + "..." : result.Token)}\nUserName: {result.UserName}\nUserId: {result.UserId}";
                
                if (result.Success)
                {
                    TempData["LoginResult"] += "\n\n✅ Login API call successful!";
                } else {
                    TempData["ErrorMessage"] = $"Login failed: {result.Message}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Simple login test error");
                TempData["ErrorMessage"] = $"Exception: {ex.Message}";
            }
            
            return RedirectToAction("SimpleTest");
        }

        // Test answer posting
        public IActionResult TestAnswer()
        {
            return View();
        }

        // Debug action to test the new API method
        public async Task<IActionResult> TestApiMethod()
        {
            try
            {
                var questions = await _apiService.GetQuestionsWithAnswersAsync("", "", 1, 5);
                return Json(new { 
                    Success = true, 
                    Count = questions.Count, 
                    Questions = questions.Select(q => new { 
                        q.Id, 
                        q.Title, 
                        q.Username, 
                        AnswerCount = q.Answers?.Count ?? 0,
                        Answers = q.Answers?.Select(a => new { a.Id, a.Username, a.Body }).ToList()
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Error = ex.Message });
            }
        }

        // Debug endpoint to show available login credentials
        public IActionResult LoginHelp()
        {
            var availableUsers = new[]
            {
                new { Email = "john@queryhub.com", Password = "password123", Name = "John Doe" },
                new { Email = "jane@queryhub.com", Password = "password123", Name = "Jane Smith" },
                new { Email = "bob@queryhub.com", Password = "password123", Name = "Bob Johnson" }
            };
            
            return Json(new { 
                Message = "Available test accounts for login:", 
                Users = availableUsers,
                Note = "All accounts use password: password123" 
            });
        }
    }
}
