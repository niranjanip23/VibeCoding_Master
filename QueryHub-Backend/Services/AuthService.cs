using BCrypt.Net;
using Microsoft.IdentityModel.Tokens;
using QueryHub_Backend.DTOs;
using QueryHub_Backend.Interfaces;
using QueryHub_Backend.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace QueryHub_Backend.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public AuthService(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                // Check if user already exists
                var existingUser = await _userRepository.GetByEmailAsync(registerDto.Email);
                if (existingUser != null)
                {
                    throw new InvalidOperationException("User with this email already exists");
                }

                existingUser = await _userRepository.GetByUsernameAsync(registerDto.Username);
                if (existingUser != null)
                {
                    throw new InvalidOperationException("User with this username already exists");
                }

                // Create new user
                var user = new User
                {
                    Name = registerDto.Name,
                    Username = registerDto.Username,
                    Email = registerDto.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                    Department = registerDto.Department,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Reputation = 0,
                    IsActive = true
                };

                var createdUser = await _userRepository.CreateAsync(user);
                var token = GenerateJwtToken(createdUser);

                return new AuthResponseDto
                {
                    Token = token,
                    UserId = createdUser.Id,
                    Username = createdUser.Username,
                    Email = createdUser.Email,
                    Reputation = createdUser.Reputation
                };
            }
            catch (Exception ex)
            {
                // Log the error for debugging
                Console.WriteLine($"Registration error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(loginDto.Email);
                if (user == null)
                {
                    Console.WriteLine($"Login failed: No user found with email {loginDto.Email}");
                    throw new UnauthorizedAccessException("Incorrect email address");
                }

                if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
                {
                    Console.WriteLine($"Login failed: Password verification failed for user {loginDto.Email}");
                    throw new UnauthorizedAccessException("Incorrect password");
                }

                var token = GenerateJwtToken(user);

                return new AuthResponseDto
                {
                    Token = token,
                    UserId = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Reputation = user.Reputation
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _userRepository.GetByIdAsync(userId);
        }

        public bool ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]!);
                
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public int? GetUserIdFromToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jsonToken = tokenHandler.ReadJwtToken(token);
                
                var userIdClaim = jsonToken.Claims.FirstOrDefault(x => x.Type == "userId");
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    return userId;
                }
            }
            catch
            {
                // Token parsing failed
            }

            return null;
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]!);
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("userId", user.Id.ToString()),
                    new Claim("username", user.Username),
                    new Claim("email", user.Email),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), 
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
