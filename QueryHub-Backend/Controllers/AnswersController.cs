using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QueryHub_Backend.DTOs;
using QueryHub_Backend.Interfaces;
using System.Security.Claims;

namespace QueryHub_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnswersController : ControllerBase
    {
        private readonly IAnswerService _answerService;

        public AnswersController(IAnswerService answerService)
        {
            _answerService = answerService;
        }

        /// <summary>
        /// Get an answer by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var answer = await _answerService.GetByIdAsync(id);
                if (answer == null)
                {
                    return NotFound(new { message = "Answer not found" });
                }

                return Ok(answer);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the answer", error = ex.Message });
            }
        }

        /// <summary>
        /// Get answers for a specific question
        /// </summary>
        [HttpGet("question/{questionId}")]
        public async Task<IActionResult> GetByQuestionId(int questionId)
        {
            try
            {
                var answers = await _answerService.GetByQuestionIdAsync(questionId);
                return Ok(answers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving answers", error = ex.Message });
            }
        }

        /// <summary>
        /// Get answers by user ID
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            try
            {
                var answers = await _answerService.GetByUserIdAsync(userId);
                return Ok(answers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving user answers", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new answer
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAnswerDto createAnswerDto)
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
                    userId = 1; // Default user for anonymous answers
                }

                var answer = await _answerService.CreateAsync(createAnswerDto, userId.Value);
                return CreatedAtAction(nameof(GetById), new { id = answer.Id }, answer);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the answer", error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing answer
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAnswerDto updateAnswerDto)
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

                var answer = await _answerService.UpdateAsync(id, updateAnswerDto, userId.Value);
                return Ok(answer);
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
                return StatusCode(500, new { message = "An error occurred while updating the answer", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete an answer
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

                await _answerService.DeleteAsync(id, userId.Value);
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
                return StatusCode(500, new { message = "An error occurred while deleting the answer", error = ex.Message });
            }
        }

        /// <summary>
        /// Mark an answer as accepted
        /// </summary>
        [HttpPost("{id}/accept")]
        [Authorize]
        public async Task<IActionResult> MarkAsAccepted(int id)
        {
            try
            {
                var userId = GetUserIdFromClaims();
                if (!userId.HasValue)
                {
                    return Unauthorized(new { message = "User ID not found in token" });
                }

                var answer = await _answerService.MarkAsAcceptedAsync(id, userId.Value);
                return Ok(answer);
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
                return StatusCode(500, new { message = "An error occurred while accepting the answer", error = ex.Message });
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
