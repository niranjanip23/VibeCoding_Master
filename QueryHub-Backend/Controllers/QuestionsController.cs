using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QueryHub_Backend.DTOs;
using QueryHub_Backend.Interfaces;
using System.Security.Claims;

namespace QueryHub_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuestionsController : ControllerBase
    {
        private readonly IQuestionService _questionService;

        public QuestionsController(IQuestionService questionService)
        {
            _questionService = questionService;
        }

        /// <summary>
        /// Get all questions
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search = null, [FromQuery] string? tag = null)
        {
            try
            {
                IEnumerable<QuestionDto> questions;

                if (!string.IsNullOrEmpty(search))
                {
                    questions = await _questionService.SearchAsync(search);
                }
                else if (!string.IsNullOrEmpty(tag))
                {
                    questions = await _questionService.GetByTagAsync(tag);
                }
                else
                {
                    questions = await _questionService.GetAllAsync();
                }

                return Ok(questions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving questions", error = ex.Message });
            }
        }

        /// <summary>
        /// Get a question by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var question = await _questionService.GetByIdAsync(id);
                if (question == null)
                {
                    return NotFound(new { message = "Question not found" });
                }

                return Ok(question);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the question", error = ex.Message });
            }
        }

        /// <summary>
        /// Get questions by user ID
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            try
            {
                var questions = await _questionService.GetByUserIdAsync(userId);
                return Ok(questions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving user questions", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new question
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateQuestionDto createQuestionDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetUserIdFromClaims();
                // If no user is authenticated, use a default user ID (1) for anonymous posting
                if (!userId.HasValue)
                {
                    userId = 1; // Default user for anonymous questions
                }

                var question = await _questionService.CreateAsync(createQuestionDto, userId.Value);
                return CreatedAtAction(nameof(GetById), new { id = question.Id }, question);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the question", error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing question
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateQuestionDto updateQuestionDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetUserIdFromClaims();
                if (!userId.HasValue)
                {
                    return Unauthorized(new { message = "User ID not found in token" });
                }

                var question = await _questionService.UpdateAsync(id, updateQuestionDto, userId.Value);
                return Ok(question);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the question", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a question
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userId = GetUserIdFromClaims();
                if (!userId.HasValue)
                {
                    return Unauthorized(new { message = "User ID not found in token" });
                }

                await _questionService.DeleteAsync(id, userId.Value);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the question", error = ex.Message });
            }
        }

        private int? GetUserIdFromClaims()
        {
            var userIdClaim = User.FindFirst("userId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return null;
        }
    }
}
