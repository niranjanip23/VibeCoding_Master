using QueryHub_Backend.Models;

namespace QueryHub_Backend.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByEmailAsync(string email);
        Task<User> CreateAsync(User user);
        Task<User> UpdateAsync(User user);
        Task DeleteAsync(int id);
        Task<IEnumerable<User>> GetAllAsync();
        Task UpdateReputationAsync(int userId, int reputationChange);
    }
    
    public interface IQuestionRepository
    {
        Task<Question?> GetByIdAsync(int id);
        Task<IEnumerable<Question>> GetAllAsync();
        Task<IEnumerable<Question>> GetByUserIdAsync(int userId);
        Task<IEnumerable<Question>> SearchAsync(string searchTerm);
        Task<IEnumerable<Question>> GetByTagAsync(string tagName);
        Task<Question> CreateAsync(Question question);
        Task<Question> UpdateAsync(Question question);
        Task DeleteAsync(int id);
        Task IncrementViewsAsync(int questionId);
        Task UpdateVotesAsync(int questionId, int voteChange);
    }
    
    public interface IAnswerRepository
    {
        Task<Answer?> GetByIdAsync(int id);
        Task<IEnumerable<Answer>> GetByQuestionIdAsync(int questionId);
        Task<IEnumerable<Answer>> GetByUserIdAsync(int userId);
        Task<Answer> CreateAsync(Answer answer);
        Task<Answer> UpdateAsync(Answer answer);
        Task DeleteAsync(int id);
        Task UpdateVotesAsync(int answerId, int voteChange);
        Task MarkAsAcceptedAsync(int answerId, int questionId);
    }
    
    public interface IVoteRepository
    {
        Task<Vote?> GetByUserAndQuestionAsync(int userId, int questionId);
        Task<Vote?> GetByUserAndAnswerAsync(int userId, int answerId);
        Task<IEnumerable<Vote>> GetByUserIdAsync(int userId);
        Task<Vote> CreateAsync(Vote vote);
        Task<Vote> UpdateAsync(Vote vote);
        Task DeleteAsync(int id);
        Task<int> GetQuestionVoteCountAsync(int questionId);
        Task<int> GetAnswerVoteCountAsync(int answerId);
    }
    
    public interface ITagRepository
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
    }
    
    public interface ICommentRepository
    {
        Task<Comment?> GetByIdAsync(int id);
        Task<IEnumerable<Comment>> GetByQuestionIdAsync(int questionId);
        Task<IEnumerable<Comment>> GetByAnswerIdAsync(int answerId);
        Task<IEnumerable<Comment>> GetByUserIdAsync(int userId);
        Task<Comment> CreateAsync(Comment comment);
        Task<Comment> UpdateAsync(Comment comment);
        Task DeleteAsync(int id);
    }
}
