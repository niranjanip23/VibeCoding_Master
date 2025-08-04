using Microsoft.AspNetCore.Mvc;
using QueryHub_Backend.DTOs;
using QueryHub_Backend.Interfaces;

namespace QueryHub_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _authService.RegisterAsync(registerDto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during registration", error = ex.Message });
            }
        }

        /// <summary>
        /// Login with email and password
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _authService.LoginAsync(loginDto);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during login", error = ex.Message });
            }
        }

        /// <summary>
        /// Validate a JWT token
        /// </summary>
        [HttpPost("validate-token")]
        public async Task<IActionResult> ValidateToken([FromBody] ValidateTokenDto validateTokenDto)
        {
            try
            {
                if (string.IsNullOrEmpty(validateTokenDto.Token))
                {
                    return BadRequest(new { message = "Token is required" });
                }

                var isValid = _authService.ValidateToken(validateTokenDto.Token);
                
                if (isValid)
                {
                    var userId = _authService.GetUserIdFromToken(validateTokenDto.Token);
                    if (userId.HasValue)
                    {
                        var user = await _authService.GetUserByIdAsync(userId.Value);
                        if (user != null)
                        {
                            return Ok(new { 
                                isValid = true, 
                                userId = user.Id,
                                username = user.Username,
                                email = user.Email,
                                reputation = user.Reputation
                            });
                        }
                    }
                }

                return Ok(new { isValid = false });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during token validation", error = ex.Message });
            }
        }
    }

    public class ValidateTokenDto
    {
        public string Token { get; set; } = string.Empty;
    }
}
