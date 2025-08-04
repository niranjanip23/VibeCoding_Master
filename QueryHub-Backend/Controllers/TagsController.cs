using Microsoft.AspNetCore.Mvc;
using QueryHub_Backend.Interfaces;

namespace QueryHub_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TagsController : ControllerBase
    {
        private readonly ITagService _tagService;

        public TagsController(ITagService tagService)
        {
            _tagService = tagService;
        }

        /// <summary>
        /// Get all tags
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(search))
                {
                    var searchResults = await _tagService.SearchAsync(search);
                    return Ok(searchResults);
                }

                var tags = await _tagService.GetAllAsync();
                return Ok(tags);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving tags", error = ex.Message });
            }
        }

        /// <summary>
        /// Get a tag by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var tag = await _tagService.GetByIdAsync(id);
                if (tag == null)
                {
                    return NotFound(new { message = "Tag not found" });
                }

                return Ok(tag);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the tag", error = ex.Message });
            }
        }

        /// <summary>
        /// Get a tag by name
        /// </summary>
        [HttpGet("name/{name}")]
        public async Task<IActionResult> GetByName(string name)
        {
            try
            {
                var tag = await _tagService.GetByNameAsync(name);
                if (tag == null)
                {
                    return NotFound(new { message = "Tag not found" });
                }

                return Ok(tag);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the tag", error = ex.Message });
            }
        }

        /// <summary>
        /// Get tags for a specific question
        /// </summary>
        [HttpGet("question/{questionId}")]
        public async Task<IActionResult> GetByQuestionId(int questionId)
        {
            try
            {
                var tags = await _tagService.GetByQuestionIdAsync(questionId);
                return Ok(tags);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving question tags", error = ex.Message });
            }
        }

        /// <summary>
        /// Get popular tags
        /// </summary>
        [HttpGet("popular")]
        public async Task<IActionResult> GetPopular([FromQuery] int limit = 10)
        {
            try
            {
                var tags = await _tagService.GetPopularTagsAsync(limit);
                return Ok(tags);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving popular tags", error = ex.Message });
            }
        }

        /// <summary>
        /// Get question count for a tag
        /// </summary>
        [HttpGet("{id}/question-count")]
        public async Task<IActionResult> GetQuestionCount(int id)
        {
            try
            {
                var count = await _tagService.GetQuestionCountByTagAsync(id);
                return Ok(new { tagId = id, questionCount = count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving question count", error = ex.Message });
            }
        }
    }
}
