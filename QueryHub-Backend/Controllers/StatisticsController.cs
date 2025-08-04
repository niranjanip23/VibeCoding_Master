using Microsoft.AspNetCore.Mvc;
using QueryHub_Backend.Interfaces;
using QueryHub_Backend.Repositories;

namespace QueryHub_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatisticsController : ControllerBase
    {
        private readonly IQuestionService _questionService;
        private readonly IAnswerService _answerService;
        private readonly ITagService _tagService;
        private readonly IUserRepository _userRepository;

        public StatisticsController(
            IQuestionService questionService,
            IAnswerService answerService,
            ITagService tagService,
            IUserRepository userRepository)
        {
            _questionService = questionService;
            _answerService = answerService;
            _tagService = tagService;
            _userRepository = userRepository;
        }

        /// <summary>
        /// Get dashboard statistics
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardStatistics()
        {
            try
            {
                // Get total questions count
                var questions = await _questionService.GetAllAsync();
                var totalQuestions = questions.Count();

                // Get total answers count - need to count all answers across all questions
                int totalAnswers = 0;
                foreach (var question in questions)
                {
                    var questionAnswers = await _answerService.GetByQuestionIdAsync(question.Id);
                    totalAnswers += questionAnswers.Count();
                }

                // Get active users (users who have registered in last 30 days or have recent activity)
                var allUsers = await _userRepository.GetAllAsync();
                var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
                var activeUsers = allUsers.Count(u => u.CreatedAt >= thirtyDaysAgo);
                
                // If no recent users, show total users to avoid showing 0
                if (activeUsers == 0)
                {
                    activeUsers = allUsers.Count();
                }

                // Get total tags count
                var tags = await _tagService.GetAllAsync();
                var totalTags = tags.Count();

                var statistics = new
                {
                    TotalQuestions = totalQuestions,
                    TotalAnswers = totalAnswers,
                    ActiveUsers = activeUsers,
                    TotalTags = totalTags
                };

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving statistics", error = ex.Message });
            }
        }
    }
}
