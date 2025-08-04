using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QueryHub_Frontend.Models;
using QueryHub_Frontend.Services;
using System.Security.Claims;

namespace QueryHub_Frontend.Controllers
{
    public class AnswersController : Controller
    {
        private readonly ILogger<AnswersController> _logger;
        private readonly IApiService _apiService;

        public AnswersController(ILogger<AnswersController> logger, IApiService apiService)
        {
            _logger = logger;
            _apiService = apiService;
        }

        // POST: Answers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int questionId, string content)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(content))
                {
                    TempData["ErrorMessage"] = "Answer content cannot be empty.";
                    return RedirectToAction("Details", "Questions", new { id = questionId });
                }

                // Check minimum length requirement (10 characters as per DTO)
                if (content.Length < 10)
                {
                    TempData["ErrorMessage"] = "Answer must be at least 10 characters long.";
                    return RedirectToAction("Details", "Questions", new { id = questionId });
                }

                // Call the API to create the answer (allow anonymous posting)
                var answer = await _apiService.CreateAnswerAsync(questionId, content);
                
                if (answer != null)
                {
                    TempData["SuccessMessage"] = "Your answer has been posted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to post your answer. Please try again.";
                }

                return RedirectToAction("Details", "Questions", new { id = questionId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating answer for question {QuestionId}", questionId);
                TempData["ErrorMessage"] = "An error occurred while posting your answer. Please try again.";
                return RedirectToAction("Details", "Questions", new { id = questionId });
            }
        }

        // POST: Answers/Vote
        [HttpPost]
        [Authorize]
        public IActionResult Vote(int id, bool isUpvote)
        {
            // In real app, update database
            var currentVotes = new Random().Next(1, 15);
            var newVotes = isUpvote ? currentVotes + 1 : Math.Max(0, currentVotes - 1);
            
            return Json(new { success = true, votes = newVotes });
        }

        // POST: Answers/Accept
        [HttpPost]
        [Authorize]
        public IActionResult Accept(int id)
        {
            // In real app, update database and check if user owns the question
            return Json(new { success = true, message = "Answer accepted!" });
        }

        // POST: Answers/AddComment
        [HttpPost]
        [Authorize]
        public IActionResult AddComment(int answerId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return Json(new { success = false, message = "Comment cannot be empty" });
            }

            var comment = new Comment
            {
                Id = new Random().Next(1000, 9999),
                Content = content,
                AnswerId = answerId,
                UserId = int.Parse(User.FindFirst("UserId")?.Value ?? "1"),
                UserName = User.Identity!.Name ?? "Demo User",
                CreatedDate = DateTime.Now
            };

            return Json(new { 
                success = true, 
                comment = new {
                    id = comment.Id,
                    content = comment.Content,
                    userName = comment.UserName,
                    createdDate = comment.CreatedDate.ToString("MMM dd, yyyy 'at' HH:mm")
                }
            });
        }
    }
}
