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
        [Authorize]
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

                // User must be authenticated to reach this point due to [Authorize] attribute
                var token = User.FindFirst("Token")?.Value;
                if (string.IsNullOrEmpty(token))
                {
                    // User is authenticated but token is missing - redirect to login
                    TempData["ErrorMessage"] = "Your session has expired. Please log in again to post an answer.";
                    return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Details", "Questions", new { id = questionId }) });
                }

                // Call the authenticated API to create the answer
                var answer = await _apiService.CreateAnswerAsync(questionId, content, token);
                
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
            catch (UnauthorizedAccessException)
            {
                // Handle authentication errors specifically
                TempData["ErrorMessage"] = "Your session has expired. Please log in again to post an answer.";
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Details", "Questions", new { id = questionId }) });
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
        public async Task<IActionResult> Vote(int id, bool isUpvote)
        {
            try
            {
                var token = User.FindFirst("Token")?.Value;
                if (string.IsNullOrEmpty(token))
                {
                    return Json(new { success = false, message = "Authentication required" });
                }

                // For answer voting, we need to get the questionId first, but we'll use the answerId
                // The API service will handle this correctly
                var success = await _apiService.VoteAsync(0, id, isUpvote, token); // questionId=0, answerId=id
                
                if (success)
                {
                    // Get updated vote count for the answer
                    var voteCount = await _apiService.GetAnswerVoteCountAsync(id, token);
                    return Json(new { success = true, votes = voteCount ?? 0 });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to record vote" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error voting on answer {AnswerId}", id);
                return Json(new { success = false, message = "An error occurred while voting" });
            }
        }

        // POST: Answers/Accept
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Accept(int id)
        {
            try
            {
                var token = User.FindFirst("Token")?.Value;
                if (string.IsNullOrEmpty(token))
                {
                    return Json(new { success = false, message = "Authentication required" });
                }

                var success = await _apiService.AcceptAnswerAsync(id, token);
                
                if (success)
                {
                    return Json(new { success = true, message = "Answer accepted! The author will receive bonus points." });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to accept answer. You can only accept answers to your own questions." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accepting answer {AnswerId}", id);
                return Json(new { success = false, message = "An error occurred while accepting the answer" });
            }
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
