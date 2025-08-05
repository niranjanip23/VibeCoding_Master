using QueryHub_Frontend.Models;
using System.Text;
using System.Text.Json;

namespace QueryHub_Frontend.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ApiService> _logger;
        private readonly string _baseUrl;

        public ApiService(HttpClient httpClient, IConfiguration configuration, ILogger<ApiService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5031";
            
            // Configure HttpClient
            _httpClient.BaseAddress = new Uri(_baseUrl);
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public async Task<AuthResult> LoginAsync(string email, string password)
        {
            try
            {
                var loginRequest = new { Email = email, Password = password };
                var json = JsonSerializer.Serialize(loginRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation("Attempting login for {Email}", email);
                var response = await _httpClient.PostAsync("/api/auth/login", content);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                _logger.LogInformation("Login response status: {StatusCode}, Content: {Content}", 
                    response.StatusCode, responseContent);

                if (response.IsSuccessStatusCode)
                {
                    var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return new AuthResult
                    {
                        Success = true,
                        Token = loginResponse?.Token,
                        UserName = loginResponse?.Username,
                        UserId = loginResponse?.UserId.ToString()
                    };
                }
                else
                {
                    return new AuthResult
                    {
                        Success = false,
                        Message = $"Login failed: {response.StatusCode} - {responseContent}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login error for {Email}", email);
                return new AuthResult
                {
                    Success = false,
                    Message = $"Login error: {ex.Message}"
                };
            }
        }

        public async Task<AuthResult> RegisterAsync(string name, string username, string email, string password, string department)
        {
            try
            {
                var registerRequest = new { Name = name, Username = username, Email = email, Password = password, Department = department };
                var json = JsonSerializer.Serialize(registerRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/auth/register", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var registerResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return new AuthResult
                    {
                        Success = true,
                        Token = registerResponse?.Token,
                        UserName = registerResponse?.Username,
                        UserId = registerResponse?.UserId.ToString(),
                        Message = "Registration successful"
                    };
                }
                else
                {
                    return new AuthResult
                    {
                        Success = false,
                        Message = $"Registration failed: {response.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new AuthResult
                {
                    Success = false,
                    Message = $"Registration error: {ex.Message}"
                };
            }
        }

        public async Task<List<QuestionViewModel>> GetQuestionsAsync(string search = "", string tag = "", int page = 1, int pageSize = 10)
        {
            try
            {
                var queryParams = new List<string>();
                if (!string.IsNullOrEmpty(search)) queryParams.Add($"search={Uri.EscapeDataString(search)}");
                if (!string.IsNullOrEmpty(tag)) queryParams.Add($"tag={Uri.EscapeDataString(tag)}");
                queryParams.Add($"page={page}");
                queryParams.Add($"pageSize={pageSize}");

                var query = string.Join("&", queryParams);
                var url = $"/api/questions?{query}";

                var response = await _httpClient.GetAsync(url);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var questions = JsonSerializer.Deserialize<List<QuestionApiModel>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return questions?.Select(MapToQuestionViewModel).ToList() ?? new List<QuestionViewModel>();
                }
                else
                {
                    return new List<QuestionViewModel>();
                }
            }
            catch (Exception)
            {
                return new List<QuestionViewModel>();
            }
        }

        public async Task<List<QuestionApiModel>> GetQuestionsWithAnswersAsync(string search = "", string tag = "", int page = 1, int pageSize = 10)
        {
            try
            {
                var queryParams = new List<string>();
                if (!string.IsNullOrEmpty(search)) queryParams.Add($"search={Uri.EscapeDataString(search)}");
                if (!string.IsNullOrEmpty(tag)) queryParams.Add($"tag={Uri.EscapeDataString(tag)}");
                queryParams.Add($"page={page}");
                queryParams.Add($"pageSize={pageSize}");
                queryParams.Add("includeAnswers=true"); // Request full answer details

                var queryString = string.Join("&", queryParams);
                var response = await _httpClient.GetAsync($"/api/questions?{queryString}");
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var questions = JsonSerializer.Deserialize<List<QuestionApiModel>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return questions ?? new List<QuestionApiModel>();
                }
                else
                {
                    return new List<QuestionApiModel>();
                }
            }
            catch (Exception)
            {
                return new List<QuestionApiModel>();
            }
        }

        public async Task<QuestionDetailViewModel?> GetQuestionAsync(int id, string? token = null)
        {
            try
            {
                // Set authorization header if token is provided
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
                else
                {
                    // Clear authorization header for anonymous access
                    _httpClient.DefaultRequestHeaders.Authorization = null;
                }

                var response = await _httpClient.GetAsync($"/api/questions/{id}");
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var question = JsonSerializer.Deserialize<QuestionDetailApiModel>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return question != null ? MapToQuestionDetailViewModel(question) : null;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<string>> GetTagsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/tags");
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var tags = JsonSerializer.Deserialize<List<TagApiModel>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return tags?.Select(t => t.Name).ToList() ?? new List<string>();
                }
                else
                {
                    return new List<string>();
                }
            }
            catch (Exception)
            {
                return new List<string>();
            }
        }

        public async Task<QuestionViewModel?> CreateQuestionAsync(string title, string description, List<string> tags, string token)
        {
            try
            {
                _logger.LogInformation("Creating question with title: {Title}", title);
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var createRequest = new { Title = title, Body = description, Tags = tags };
                var json = JsonSerializer.Serialize(createRequest);
                _logger.LogInformation("Question request JSON: {Json}", json);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/questions", content);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                _logger.LogInformation("Question creation response - Status: {StatusCode}, Content: {Content}", 
                    response.StatusCode, responseContent);

                if (response.IsSuccessStatusCode)
                {
                    var question = JsonSerializer.Deserialize<QuestionApiModel>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (question != null)
                    {
                        _logger.LogInformation("Successfully deserialized question with ID: {QuestionId}", question.Id);
                        return MapToQuestionViewModel(question);
                    }
                    else
                    {
                        _logger.LogError("Failed to deserialize question response: {Content}", responseContent);
                        return null;
                    }
                }
                else
                {
                    var errorMessage = $"HTTP {response.StatusCode}: {responseContent}";
                    _logger.LogError("Question creation failed: {Error}", errorMessage);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during question creation");
                return null;
            }
            finally
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        public async Task<AnswerViewModel?> CreateAnswerAsync(int questionId, string content, string token)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var createRequest = new { QuestionId = questionId, Body = content };
                var json = JsonSerializer.Serialize(createRequest);
                var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/answers", stringContent);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var answer = JsonSerializer.Deserialize<AnswerApiModel>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return answer != null ? MapToAnswerViewModel(answer) : null;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    // Throw a specific exception for authentication issues
                    throw new UnauthorizedAccessException("Authentication required to post an answer.");
                }
                else
                {
                    return null;
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Re-throw authentication exceptions
                throw;
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        public async Task<bool> VoteAsync(int questionId, int? answerId, bool isUpvote, string token)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                string endpoint;
                object voteRequest;
                
                if (answerId.HasValue)
                {
                    // Vote on answer
                    endpoint = "/api/votes/answer";
                    voteRequest = new { 
                        AnswerId = answerId.Value, 
                        VoteType = isUpvote ? 1 : -1  // Send as integer (1 = Upvote, -1 = Downvote)
                    };
                }
                else
                {
                    // Vote on question
                    endpoint = "/api/votes/question";
                    voteRequest = new { 
                        QuestionId = questionId, 
                        VoteType = isUpvote ? 1 : -1  // Send as integer (1 = Upvote, -1 = Downvote)
                    };
                }

                var json = JsonSerializer.Serialize(voteRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation("Sending vote request to {Endpoint}: {Json}", endpoint, json);
                var response = await _httpClient.PostAsync(endpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                _logger.LogInformation("Vote response status: {StatusCode}, Content: {Content}", 
                    response.StatusCode, responseContent);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Vote failed: {StatusCode} - {Content}", response.StatusCode, responseContent);
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in VoteAsync");
                return false;
            }
            finally
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        public async Task<int?> GetQuestionVoteCountAsync(int questionId, string token)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync($"/api/votes/question/{questionId}/count");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<JsonElement>(json);
                    
                    if (result.TryGetProperty("voteCount", out var voteCountElement))
                    {
                        return voteCountElement.GetInt32();
                    }
                }
                
                return null;
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        public async Task<int?> GetAnswerVoteCountAsync(int answerId, string token)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync($"/api/votes/answer/{answerId}/count");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<JsonElement>(json);
                    
                    if (result.TryGetProperty("voteCount", out var voteCountElement))
                    {
                        return voteCountElement.GetInt32();
                    }
                }
                
                return null;
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        public async Task<bool> AcceptAnswerAsync(int answerId, string token)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PostAsync($"/api/answers/{answerId}/accept", null);
                
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accepting answer {AnswerId}", answerId);
                return false;
            }
            finally
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        public async Task<DashboardStatistics?> GetDashboardStatisticsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/statistics/dashboard");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var statistics = JsonSerializer.Deserialize<DashboardStatistics>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return statistics;
                }
                
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        // Helper mapping methods
        private QuestionViewModel MapToQuestionViewModel(QuestionApiModel apiModel)
        {
            return new QuestionViewModel
            {
                Id = apiModel.Id,
                Title = apiModel.Title,
                Description = apiModel.Body,
                Tags = apiModel.Tags ?? new List<string>(),
                Author = apiModel.Username,
                CreatedAt = apiModel.CreatedAt,
                Votes = apiModel.Votes,
                AnswerVotes = apiModel.AnswerVotes,
                Answers = apiModel.Answers?.Count ?? 0, // Count the actual answers from backend
                Views = apiModel.Views
            };
        }

        private QuestionDetailViewModel MapToQuestionDetailViewModel(QuestionDetailApiModel apiModel)
        {
            return new QuestionDetailViewModel
            {
                Id = apiModel.Id,
                Title = apiModel.Title,
                Description = apiModel.Body,
                Tags = apiModel.Tags ?? new List<string>(),
                Author = apiModel.Username,
                CreatedAt = apiModel.CreatedAt,
                Votes = apiModel.Votes,
                Views = apiModel.Views,
                Answers = apiModel.Answers?.Select(MapToAnswerViewModel).ToList() ?? new List<AnswerViewModel>()
            };
        }

        private AnswerViewModel MapToAnswerViewModel(AnswerApiModel apiModel)
        {
            return new AnswerViewModel
            {
                Id = apiModel.Id,
                Content = apiModel.Body,
                Author = apiModel.Username,
                CreatedAt = apiModel.CreatedAt,
                Votes = apiModel.Votes, // Changed from VoteCount to Votes
                QuestionId = apiModel.QuestionId
            };
        }
    }

    // API model classes to match backend DTOs
    public class LoginResponse
    {
        public string? Token { get; set; }
        public string? Username { get; set; }
        public int UserId { get; set; }
    }

    public class RegisterResponse
    {
        public string? Message { get; set; }
    }
}
