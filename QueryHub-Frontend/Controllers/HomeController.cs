using Microsoft.AspNetCore.Mvc;
using QueryHub_Frontend.Models;
using QueryHub_Frontend.Services;
using System.Diagnostics;

namespace QueryHub_Frontend.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IApiService _apiService;

        public HomeController(ILogger<HomeController> logger, IApiService apiService)
        {
            _logger = logger;
            _apiService = apiService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var questions = await _apiService.GetQuestionsAsync("", "", 1, 5);
                var tags = await _apiService.GetTagsAsync();
                
                // Get all questions to calculate real statistics
                var allQuestions = await _apiService.GetQuestionsAsync("", "", 1, 1000); // Large page size to get all questions
                
                // Calculate total answers from all questions
                int totalAnswers = allQuestions.Sum(q => q.AnswerCount);
                
                // Get unique users count (approximation based on questions - could be improved)
                var uniqueUsers = allQuestions.Select(q => q.UserName).Distinct().Count();
                
                var model = new QuestionListViewModel
                {
                    Questions = questions,
                    PopularTags = tags.Take(6).ToList(),
                    TotalQuestions = allQuestions.Count,
                    TotalAnswers = totalAnswers,
                    ActiveUsers = uniqueUsers,
                    TotalTags = tags.Count
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading home page data");
                
                // Fallback to empty model
                var model = new QuestionListViewModel
                {
                    Questions = new List<QuestionViewModel>(),
                    PopularTags = new List<string>(),
                    TotalQuestions = 0,
                    TotalAnswers = 0,
                    ActiveUsers = 0,
                    TotalTags = 0
                };
                
                TempData["ErrorMessage"] = "Error loading data. Please try again.";
                return View(model);
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
