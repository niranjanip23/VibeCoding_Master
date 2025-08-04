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

        public IActionResult Profile()
        {
            if (!User.Identity!.IsAuthenticated)
            {
                return RedirectToAction("Login");
            }

            var user = new User
            {
                Id = 1,
                Name = User.Identity.Name ?? "Demo User",
                Email = User.FindFirst(ClaimTypes.Email)?.Value ?? "demo@queryhub.com",
                Department = User.FindFirst("Department")?.Value ?? "IT Department",
                Avatar = "/images/avatars/demo-user.jpg",
                Reputation = 1250,
                JoinedDate = DateTime.Now.AddMonths(-6)
            };

            return View(user);
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
    }
}
