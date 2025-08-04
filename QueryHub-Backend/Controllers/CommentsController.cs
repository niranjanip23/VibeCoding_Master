using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QueryHub_Backend.Interfaces;
using QueryHub_Backend.Models;
using System.Security.Claims;

namespace QueryHub_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentsController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        /// <summary>
        /// Get a comment by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var comment = await _commentService.GetByIdAsync(id);
                if (comment == null)
                {
                    return NotFound(new { message = "Comment not found" });
                }

                return Ok(comment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the comment", error = ex.Message });
            }
        }

        /// <summary>
        /// Get comments for a specific question
        /// </summary>
        [HttpGet("question/{questionId}")]
        public async Task<IActionResult> GetByQuestionId(int questionId)
        {
            try
            {
                var comments = await _commentService.GetByQuestionIdAsync(questionId);
                return Ok(comments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving question comments", error = ex.Message });
            }
        }

        /// <summary>
        /// Get comments for a specific answer
        /// </summary>
        [HttpGet("answer/{answerId}")]
        public async Task<IActionResult> GetByAnswerId(int answerId)
        {
            try
            {
                var comments = await _commentService.GetByAnswerIdAsync(answerId);
                return Ok(comments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving answer comments", error = ex.Message });
            }
        }

        /// <summary>
        /// Get comments by user ID
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            try
            {
                var comments = await _commentService.GetByUserIdAsync(userId);
                return Ok(comments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving user comments", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new comment
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateCommentDto createCommentDto)
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

                var comment = new Comment
                {
                    Body = createCommentDto.Body,
                    QuestionId = createCommentDto.QuestionId,
                    AnswerId = createCommentDto.AnswerId,
                    UserId = userId.Value
                };

                var createdComment = await _commentService.CreateAsync(comment);
                return CreatedAtAction(nameof(GetById), new { id = createdComment.Id }, createdComment);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the comment", error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing comment
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCommentDto updateCommentDto)
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

                var comment = new Comment
                {
                    Id = id,
                    Body = updateCommentDto.Body
                };

                var updatedComment = await _commentService.UpdateAsync(comment, userId.Value);
                return Ok(updatedComment);
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
                return StatusCode(500, new { message = "An error occurred while updating the comment", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a comment
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

                await _commentService.DeleteAsync(id, userId.Value);
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
                return StatusCode(500, new { message = "An error occurred while deleting the comment", error = ex.Message });
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

    public class CreateCommentDto
    {
        public string Body { get; set; } = string.Empty;
        public int QuestionId { get; set; }
        public int? AnswerId { get; set; }
    }

    public class UpdateCommentDto
    {
        public string Body { get; set; } = string.Empty;
    }
}
