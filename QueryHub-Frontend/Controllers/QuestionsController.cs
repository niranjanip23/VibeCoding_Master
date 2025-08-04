using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QueryHub_Frontend.Models;
using QueryHub_Frontend.Services;
using System.Security.Claims;

namespace QueryHub_Frontend.Controllers
{
    public class QuestionsController : Controller
    {
        private readonly ILogger<QuestionsController> _logger;
        private readonly IApiService _apiService;

        public QuestionsController(ILogger<QuestionsController> logger, IApiService apiService)
        {
            _logger = logger;
            _apiService = apiService;
        }

        // GET: Questions
        public async Task<IActionResult> Index(string search = "", string tag = "", int page = 1)
        {
            try
            {
                var questions = await _apiService.GetQuestionsAsync(search, tag, page, 10);
                var tags = await _apiService.GetTagsAsync();

                var model = new QuestionListViewModel
                {
                    Questions = questions,
                    SearchQuery = search,
                    SelectedTag = tag,
                    PopularTags = tags.Take(8).ToList(),
                    TotalQuestions = questions.Count,
                    CurrentPage = page,
                    PageSize = 10
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading questions");
                TempData["ErrorMessage"] = "Error loading questions. Please try again.";
                
                // Fallback to empty model
                var model = new QuestionListViewModel
                {
                    Questions = new List<QuestionViewModel>(),
                    SearchQuery = search,
                    SelectedTag = tag,
                    PopularTags = new List<string>(),
                    TotalQuestions = 0,
                    CurrentPage = page,
                    PageSize = 10
                };
                return View(model);
            }
        }

        // GET: Questions/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var question = await _apiService.GetQuestionAsync(id);
                if (question == null)
                {
                    _logger.LogWarning("Question with ID {QuestionId} not found", id);
                    return NotFound();
                }

                // Defensive check to ensure we have the correct model type
                if (question is not QuestionDetailViewModel)
                {
                    _logger.LogError("Unexpected model type returned from GetQuestionAsync: {ModelType}", question.GetType().FullName);
                    TempData["ErrorMessage"] = "Error loading question details. Please try again.";
                    return RedirectToAction("Index");
                }

                return View(question);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading question details for ID {QuestionId}", id);
                TempData["ErrorMessage"] = "Error loading question details. Please try again.";
                return RedirectToAction("Index");
            }
        }

        // GET: Questions/Ask
        [Authorize]
        public async Task<IActionResult> Ask()
        {
            try
            {
                var tags = await _apiService.GetTagsAsync();
                ViewBag.AvailableTags = tags;
                return View(new CreateQuestionViewModel());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading tags for Ask form");
                ViewBag.AvailableTags = new List<string>();
                return View(new CreateQuestionViewModel());
            }
        }

        // POST: Questions/Ask
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ask(CreateQuestionViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var token = User.FindFirst("Token")?.Value;
                    if (string.IsNullOrEmpty(token))
                    {
                        TempData["ErrorMessage"] = "Authentication token not found. Please log in again.";
                        return RedirectToAction("Login", "Account");
                    }

                    // Convert comma-separated tags string to list
                    var tagsList = string.IsNullOrWhiteSpace(model.Tags) 
                        ? new List<string>() 
                        : model.Tags.Split(',').Select(t => t.Trim()).Where(t => !string.IsNullOrEmpty(t)).ToList();

                    var question = await _apiService.CreateQuestionAsync(model.Title, model.Description, tagsList, token);
                    
                    if (question != null)
                    {
                        _logger.LogInformation("Question created successfully with ID {QuestionId}, redirecting to Details", question.Id);
                        TempData["SuccessMessage"] = "Your question has been posted successfully!";
                        
                        // Defensive approach: ensure we're redirecting with a valid question ID
                        if (question.Id > 0)
                        {
                            return RedirectToAction("Details", new { id = question.Id });
                        }
                        else
                        {
                            _logger.LogError("Question created but has invalid ID: {QuestionId}", question.Id);
                            TempData["ErrorMessage"] = "Question was created but there was an issue displaying it. Please check your questions list.";
                            return RedirectToAction("Index");
                        }
                    }
                    else
                    {
                        _logger.LogError("CreateQuestionAsync returned null for question: {Title}", model.Title);
                        ModelState.AddModelError(string.Empty, "Failed to create question. Please try again.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating question");
                    ModelState.AddModelError(string.Empty, "An error occurred while creating your question. Please try again.");
                }
            }

            // Reload tags if there was an error
            try
            {
                var tags = await _apiService.GetTagsAsync();
                ViewBag.AvailableTags = tags;
            }
            catch
            {
                ViewBag.AvailableTags = new List<string>();
            }

            return View(model);
        }

        // POST: Questions/Vote
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

                var success = await _apiService.VoteAsync(id, null, isUpvote, token);
                
                if (success)
                {
                    // Get updated vote count
                    var voteCount = await _apiService.GetQuestionVoteCountAsync(id, token);
                    return Json(new { success = true, votes = voteCount ?? 0 });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to record vote" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error voting on question {QuestionId}", id);
                return Json(new { success = false, message = "An error occurred while voting" });
            }
        }

        // POST: Questions/AddComment
        [HttpPost]
        [Authorize]
        public IActionResult AddComment(int questionId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return Json(new { success = false, message = "Comment cannot be empty" });
            }

            var comment = new Comment
            {
                Id = new Random().Next(1000, 9999),
                Content = content,
                QuestionId = questionId,
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

        private List<Question> GetSampleQuestions()
        {
            return new List<Question>
            {
                new Question
                {
                    Id = 1,
                    Title = "How to implement async/await in C#?",
                    Description = "I'm having trouble understanding how to properly use async/await in my C# application. Can someone provide a clear explanation with examples?",
                    Tags = "C#, async, await, programming",
                    UserName = "John Doe",
                    UserAvatar = "/images/avatars/user1.jpg",
                    CreatedDate = DateTime.Now.AddHours(-2),
                    Views = 45,
                    Votes = 8,
                    AnswerCount = 3,
                    IsAnswered = true
                },
                new Question
                {
                    Id = 2,
                    Title = "Best practices for ASP.NET Core MVC?",
                    Description = "What are the current best practices for developing ASP.NET Core MVC applications in 2025?",
                    Tags = "ASP.NET, MVC, best-practices",
                    UserName = "Jane Smith",
                    UserAvatar = "/images/avatars/user2.jpg",
                    CreatedDate = DateTime.Now.AddHours(-5),
                    Views = 78,
                    Votes = 12,
                    AnswerCount = 5,
                    IsAnswered = true
                },
                new Question
                {
                    Id = 3,
                    Title = "JavaScript closure explanation needed",
                    Description = "Can someone explain JavaScript closures with practical examples? I'm confused about how they work.",
                    Tags = "JavaScript, closure, functions",
                    UserName = "Mike Johnson",
                    UserAvatar = "/images/avatars/user3.jpg",
                    CreatedDate = DateTime.Now.AddHours(-1),
                    Views = 23,
                    Votes = 4,
                    AnswerCount = 1,
                    IsAnswered = false
                },
                new Question
                {
                    Id = 4,
                    Title = "SQL Query Optimization Tips",
                    Description = "Looking for tips to optimize slow SQL queries in a large database.",
                    Tags = "SQL, optimization, database, performance",
                    UserName = "Sarah Wilson",
                    UserAvatar = "/images/avatars/user4.jpg",
                    CreatedDate = DateTime.Now.AddDays(-1),
                    Views = 156,
                    Votes = 18,
                    AnswerCount = 7,
                    IsAnswered = true
                },
                new Question
                {
                    Id = 5,
                    Title = "React Hooks vs Class Components",
                    Description = "When should I use React Hooks instead of class components?",
                    Tags = "React, hooks, components, JavaScript",
                    UserName = "David Chen",
                    UserAvatar = "/images/avatars/user5.jpg",
                    CreatedDate = DateTime.Now.AddDays(-2),
                    Views = 89,
                    Votes = 11,
                    AnswerCount = 4,
                    IsAnswered = true
                }
            };
        }

        private List<Answer> GetSampleAnswers(int questionId)
        {
            if (questionId == 1)
            {
                return new List<Answer>
                {
                    new Answer
                    {
                        Id = 1,
                        Content = "Async/await is used for asynchronous programming. Here's a simple example:\n\n```csharp\npublic async Task<string> GetDataAsync()\n{\n    var result = await SomeAsyncOperation();\n    return result;\n}\n```",
                        QuestionId = questionId,
                        UserName = "Expert Developer",
                        UserAvatar = "/images/avatars/expert1.jpg",
                        CreatedDate = DateTime.Now.AddHours(-1),
                        Votes = 15,
                        IsAccepted = true,
                        Comments = GetSampleComments(answerId: 1)
                    },
                    new Answer
                    {
                        Id = 2,
                        Content = "Remember to always use ConfigureAwait(false) when you don't need to capture the synchronization context.",
                        QuestionId = questionId,
                        UserName = "Code Guru",
                        UserAvatar = "/images/avatars/expert2.jpg",
                        CreatedDate = DateTime.Now.AddMinutes(-30),
                        Votes = 8,
                        IsAccepted = false
                    }
                };
            }
            return new List<Answer>();
        }

        private List<Comment> GetSampleComments(int? questionId = null, int? answerId = null)
        {
            var comments = new List<Comment>();
            
            if (questionId.HasValue)
            {
                comments.AddRange(new List<Comment>
                {
                    new Comment
                    {
                        Id = 1,
                        Content = "Great question! I was wondering about this too.",
                        QuestionId = questionId,
                        UserName = "Alice Johnson",
                        CreatedDate = DateTime.Now.AddMinutes(-45)
                    }
                });
            }
            
            if (answerId.HasValue)
            {
                comments.AddRange(new List<Comment>
                {
                    new Comment
                    {
                        Id = 2,
                        Content = "This helped me a lot, thanks!",
                        AnswerId = answerId,
                        UserName = "Bob Smith",
                        CreatedDate = DateTime.Now.AddMinutes(-20)
                    }
                });
            }
            
            return comments;
        }
    }
}
