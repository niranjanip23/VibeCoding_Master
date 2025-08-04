using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QueryHub_Backend.DTOs;
using QueryHub_Backend.Interfaces;
using System.Security.Claims;

namespace QueryHub_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VotesController : ControllerBase
    {
        private readonly IVoteService _voteService;

        public VotesController(IVoteService voteService)
        {
            _voteService = voteService;
        }

        /// <summary>
        /// Vote on a question
        /// </summary>
        [HttpPost("question")]
        [Authorize]
        public async Task<IActionResult> VoteOnQuestion([FromBody] VoteQuestionDto voteDto)
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

                var vote = await _voteService.VoteOnQuestionAsync(voteDto, userId.Value);
                
                if (vote.IsRemoved)
                {
                    return Ok(new { message = "Vote removed", vote });
                }
                
                return Ok(new { message = "Vote recorded", vote });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while voting", error = ex.Message });
            }
        }

        /// <summary>
        /// Vote on an answer
        /// </summary>
        [HttpPost("answer")]
        [Authorize]
        public async Task<IActionResult> VoteOnAnswer([FromBody] VoteAnswerDto voteDto)
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

                var vote = await _voteService.VoteOnAnswerAsync(voteDto, userId.Value);
                
                if (vote.IsRemoved)
                {
                    return Ok(new { message = "Vote removed", vote });
                }
                
                return Ok(new { message = "Vote recorded", vote });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while voting", error = ex.Message });
            }
        }

        /// <summary>
        /// Get user's votes
        /// </summary>
        [HttpGet("user")]
        [Authorize]
        public async Task<IActionResult> GetUserVotes()
        {
            try
            {
                var userId = GetUserIdFromClaims();
                if (!userId.HasValue)
                {
                    return Unauthorized(new { message = "User ID not found in token" });
                }

                var votes = await _voteService.GetUserVotesAsync(userId.Value);
                return Ok(votes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving votes", error = ex.Message });
            }
        }

        /// <summary>
        /// Get vote count for a question
        /// </summary>
        [HttpGet("question/{questionId}/count")]
        public async Task<IActionResult> GetQuestionVoteCount(int questionId)
        {
            try
            {
                var count = await _voteService.GetQuestionVoteCountAsync(questionId);
                return Ok(new { questionId, voteCount = count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving vote count", error = ex.Message });
            }
        }

        /// <summary>
        /// Get vote count for an answer
        /// </summary>
        [HttpGet("answer/{answerId}/count")]
        public async Task<IActionResult> GetAnswerVoteCount(int answerId)
        {
            try
            {
                var count = await _voteService.GetAnswerVoteCountAsync(answerId);
                return Ok(new { answerId, voteCount = count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving vote count", error = ex.Message });
            }
        }

        /// <summary>
        /// Get user's vote on a specific question
        /// </summary>
        [HttpGet("question/{questionId}/user")]
        [Authorize]
        public async Task<IActionResult> GetUserVoteOnQuestion(int questionId)
        {
            try
            {
                var userId = GetUserIdFromClaims();
                if (!userId.HasValue)
                {
                    return Unauthorized(new { message = "User ID not found in token" });
                }

                var vote = await _voteService.GetUserVoteOnQuestionAsync(userId.Value, questionId);
                if (vote == null)
                {
                    return Ok(new { hasVoted = false });
                }

                return Ok(new { hasVoted = true, vote });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving user vote", error = ex.Message });
            }
        }

        /// <summary>
        /// Get user's vote on a specific answer
        /// </summary>
        [HttpGet("answer/{answerId}/user")]
        [Authorize]
        public async Task<IActionResult> GetUserVoteOnAnswer(int answerId)
        {
            try
            {
                var userId = GetUserIdFromClaims();
                if (!userId.HasValue)
                {
                    return Unauthorized(new { message = "User ID not found in token" });
                }

                var vote = await _voteService.GetUserVoteOnAnswerAsync(userId.Value, answerId);
                if (vote == null)
                {
                    return Ok(new { hasVoted = false });
                }

                return Ok(new { hasVoted = true, vote });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving user vote", error = ex.Message });
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
