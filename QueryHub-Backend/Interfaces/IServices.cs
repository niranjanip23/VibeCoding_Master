using QueryHub_Backend.DTOs;
using QueryHub_Backend.Models;

namespace QueryHub_Backend.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginDto loginRequest);
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerRequest);
        Task<User?> GetUserByIdAsync(int userId);
        bool ValidateToken(string token);
        int? GetUserIdFromToken(string token);
    }
    
    public interface IQuestionService
    {
        Task<QuestionDto?> GetByIdAsync(int id, int? currentUserId = null);
        Task<IEnumerable<QuestionDto>> GetAllAsync();
        Task<IEnumerable<QuestionDto>> GetByUserIdAsync(int userId);
        Task<IEnumerable<QuestionDto>> SearchAsync(string searchTerm);
        Task<IEnumerable<QuestionDto>> GetByTagAsync(string tagName);
        Task<QuestionDto> CreateAsync(CreateQuestionDto createQuestion, int userId);
        Task<QuestionDto> UpdateAsync(int id, UpdateQuestionDto updateQuestion, int userId);
        Task DeleteAsync(int id, int userId);
    }
    
    public interface IAnswerService
    {
        Task<AnswerDto?> GetByIdAsync(int id);
        Task<IEnumerable<AnswerDto>> GetByQuestionIdAsync(int questionId);
        Task<IEnumerable<AnswerDto>> GetByUserIdAsync(int userId);
        Task<AnswerDto> CreateAsync(CreateAnswerDto createAnswer, int userId);
        Task<AnswerDto> UpdateAsync(int id, UpdateAnswerDto updateAnswer, int userId);
        Task DeleteAsync(int id, int userId);
        Task<AnswerDto> MarkAsAcceptedAsync(int answerId, int userId);
    }
    
    public interface IVoteService
    {
        Task<VoteDto> VoteOnQuestionAsync(VoteQuestionDto voteDto, int userId);
        Task<VoteDto> VoteOnAnswerAsync(VoteAnswerDto voteDto, int userId);
        Task<IEnumerable<VoteDto>> GetUserVotesAsync(int userId);
        Task<int> GetQuestionVoteCountAsync(int questionId);
        Task<int> GetAnswerVoteCountAsync(int answerId);
        Task<VoteDto?> GetUserVoteOnQuestionAsync(int userId, int questionId);
        Task<VoteDto?> GetUserVoteOnAnswerAsync(int userId, int answerId);
    }
    
    public interface ITagService
    {
        Task<Tag?> GetByIdAsync(int id);
        Task<Tag?> GetByNameAsync(string name);
        Task<IEnumerable<Tag>> GetAllAsync();
        Task<IEnumerable<Tag>> GetByQuestionIdAsync(int questionId);
        Task<IEnumerable<Tag>> SearchAsync(string searchTerm);
        Task<Tag> CreateAsync(Tag tag);
        Task<Tag> UpdateAsync(Tag tag);
        Task DeleteAsync(int id);
        Task AddTagToQuestionAsync(int questionId, int tagId);
        Task RemoveTagFromQuestionAsync(int questionId, int tagId);
        Task<int> GetQuestionCountByTagAsync(int tagId);
        Task<IEnumerable<Tag>> GetPopularTagsAsync(int limit = 10);
    }
    
    public interface ICommentService
    {
        Task<Comment?> GetByIdAsync(int id);
        Task<IEnumerable<Comment>> GetByQuestionIdAsync(int questionId);
        Task<IEnumerable<Comment>> GetByAnswerIdAsync(int answerId);
        Task<IEnumerable<Comment>> GetByUserIdAsync(int userId);
        Task<Comment> CreateAsync(Comment comment);
        Task<Comment> UpdateAsync(Comment comment, int userId);
        Task DeleteAsync(int id, int userId);
    }
}
