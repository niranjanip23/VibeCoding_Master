using QueryHub_Frontend.Models;

namespace QueryHub_Frontend.Services
{
    public interface IApiService
    {
        Task<AuthResult> LoginAsync(string email, string password);
        Task<AuthResult> RegisterAsync(string name, string username, string email, string password, string department);
        Task<List<QuestionViewModel>> GetQuestionsAsync(string search = "", string tag = "", int page = 1, int pageSize = 10);
        Task<QuestionDetailViewModel?> GetQuestionAsync(int id);
        Task<List<string>> GetTagsAsync();
        Task<QuestionViewModel?> CreateQuestionAsync(string title, string description, List<string> tags, string token);
        Task<AnswerViewModel?> CreateAnswerAsync(int questionId, string content, string token);
        Task<AnswerViewModel?> CreateAnswerAsync(int questionId, string content); // Anonymous version
        Task<bool> VoteAsync(int questionId, int? answerId, bool isUpvote, string token);
    }

    public class AuthResult
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public string? Message { get; set; }
        public string? UserName { get; set; }
        public string? UserId { get; set; }
    }
}
