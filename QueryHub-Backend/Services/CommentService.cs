using QueryHub_Backend.Interfaces;
using QueryHub_Backend.Models;

namespace QueryHub_Backend.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly IAnswerRepository _answerRepository;
        private readonly IUserRepository _userRepository;

        public CommentService(
            ICommentRepository commentRepository,
            IQuestionRepository questionRepository,
            IAnswerRepository answerRepository,
            IUserRepository userRepository)
        {
            _commentRepository = commentRepository;
            _questionRepository = questionRepository;
            _answerRepository = answerRepository;
            _userRepository = userRepository;
        }

        public async Task<Comment?> GetByIdAsync(int id)
        {
            return await _commentRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Comment>> GetByQuestionIdAsync(int questionId)
        {
            return await _commentRepository.GetByQuestionIdAsync(questionId);
        }

        public async Task<IEnumerable<Comment>> GetByAnswerIdAsync(int answerId)
        {
            return await _commentRepository.GetByAnswerIdAsync(answerId);
        }

        public async Task<IEnumerable<Comment>> GetByUserIdAsync(int userId)
        {
            return await _commentRepository.GetByUserIdAsync(userId);
        }

        public async Task<Comment> CreateAsync(Comment comment)
        {
            // Validate that either QuestionId or AnswerId is provided, but not both
            if (comment.AnswerId.HasValue && comment.QuestionId > 0)
            {
                // If both are provided, this is a comment on an answer
                var answer = await _answerRepository.GetByIdAsync(comment.AnswerId.Value);
                if (answer == null)
                {
                    throw new ArgumentException("Answer not found");
                }
                // Ensure QuestionId matches the answer's QuestionId
                comment.QuestionId = answer.QuestionId;
            }
            else if (!comment.AnswerId.HasValue && comment.QuestionId > 0)
            {
                // This is a comment on a question
                var question = await _questionRepository.GetByIdAsync(comment.QuestionId.Value);
                if (question == null)
                {
                    throw new ArgumentException("Question not found");
                }
            }
            else
            {
                throw new ArgumentException("Comment must be associated with either a question or an answer");
            }

            comment.CreatedAt = DateTime.UtcNow;
            var createdComment = await _commentRepository.CreateAsync(comment);

            // Award reputation to the user for commenting
            await _userRepository.UpdateReputationAsync(comment.UserId, 1);

            return createdComment;
        }

        public async Task<Comment> UpdateAsync(Comment comment, int userId)
        {
            var existingComment = await _commentRepository.GetByIdAsync(comment.Id);
            if (existingComment == null)
            {
                throw new ArgumentException("Comment not found");
            }

            if (existingComment.UserId != userId)
            {
                throw new UnauthorizedAccessException("Only the comment author can edit this comment");
            }

            existingComment.Body = comment.Body;
            return await _commentRepository.UpdateAsync(existingComment);
        }

        public async Task DeleteAsync(int id, int userId)
        {
            var comment = await _commentRepository.GetByIdAsync(id);
            if (comment == null)
            {
                throw new ArgumentException("Comment not found");
            }

            if (comment.UserId != userId)
            {
                throw new UnauthorizedAccessException("Only the comment author can delete this comment");
            }

            await _commentRepository.DeleteAsync(id);

            // Deduct reputation from the user for deleting a comment
            await _userRepository.UpdateReputationAsync(userId, -1);
        }
    }
}
