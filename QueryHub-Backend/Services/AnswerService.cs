using QueryHub_Backend.DTOs;
using QueryHub_Backend.Interfaces;
using QueryHub_Backend.Models;

namespace QueryHub_Backend.Services
{
    public class AnswerService : IAnswerService
    {
        private readonly IAnswerRepository _answerRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly IUserRepository _userRepository;

        public AnswerService(
            IAnswerRepository answerRepository,
            IQuestionRepository questionRepository,
            IUserRepository userRepository)
        {
            _answerRepository = answerRepository;
            _questionRepository = questionRepository;
            _userRepository = userRepository;
        }

        public async Task<AnswerDto?> GetByIdAsync(int id)
        {
            var answer = await _answerRepository.GetByIdAsync(id);
            if (answer == null) return null;

            return new AnswerDto
            {
                Id = answer.Id,
                Body = answer.Body,
                QuestionId = answer.QuestionId,
                UserId = answer.UserId,
                CreatedAt = answer.CreatedAt,
                UpdatedAt = answer.UpdatedAt,
                Votes = answer.VoteCount,
                IsAccepted = answer.IsAccepted
            };
        }

        public async Task<IEnumerable<AnswerDto>> GetByQuestionIdAsync(int questionId)
        {
            var answers = await _answerRepository.GetByQuestionIdAsync(questionId);
            var answerDtos = new List<AnswerDto>();
            
            foreach (var answer in answers)
            {
                var user = await _userRepository.GetByIdAsync(answer.UserId);
                answerDtos.Add(new AnswerDto
                {
                    Id = answer.Id,
                    Body = answer.Body,
                    QuestionId = answer.QuestionId,
                    UserId = answer.UserId,
                    Username = user?.Username ?? "Unknown User",
                    CreatedAt = answer.CreatedAt,
                    UpdatedAt = answer.UpdatedAt,
                    Votes = answer.VoteCount,
                    IsAccepted = answer.IsAccepted
                });
            }
            
            return answerDtos;
        }

        public async Task<IEnumerable<AnswerDto>> GetByUserIdAsync(int userId)
        {
            var answers = await _answerRepository.GetByUserIdAsync(userId);
            return answers.Select(answer => new AnswerDto
            {
                Id = answer.Id,
                Body = answer.Body,
                QuestionId = answer.QuestionId,
                UserId = answer.UserId,
                CreatedAt = answer.CreatedAt,
                UpdatedAt = answer.UpdatedAt,
                Votes = answer.VoteCount,
                IsAccepted = answer.IsAccepted
            });
        }

        public async Task<AnswerDto> CreateAsync(CreateAnswerDto createAnswerDto, int userId)
        {
            // Verify that the question exists
            var question = await _questionRepository.GetByIdAsync(createAnswerDto.QuestionId);
            if (question == null)
            {
                throw new ArgumentException("Question not found");
            }

            var answer = new Answer
            {
                Content = createAnswerDto.Body, // Use Body as Content
                Body = createAnswerDto.Body,
                QuestionId = createAnswerDto.QuestionId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                VoteCount = 0,
                IsAccepted = false,
                IsActive = true
            };

            var createdAnswer = await _answerRepository.CreateAsync(answer);

            // Award reputation to the user for answering
            await _userRepository.UpdateReputationAsync(userId, 1);

            // Get the user information to populate the username
            var user = await _userRepository.GetByIdAsync(userId);

            return new AnswerDto
            {
                Id = createdAnswer.Id,
                Body = createdAnswer.Body,
                QuestionId = createdAnswer.QuestionId,
                UserId = createdAnswer.UserId,
                Username = user?.Username ?? "Unknown User",
                CreatedAt = createdAnswer.CreatedAt,
                UpdatedAt = createdAnswer.UpdatedAt,
                Votes = createdAnswer.VoteCount,
                IsAccepted = createdAnswer.IsAccepted
            };
        }

        public async Task<AnswerDto> UpdateAsync(int id, UpdateAnswerDto updateAnswerDto, int userId)
        {
            var answer = await _answerRepository.GetByIdAsync(id);
            if (answer == null)
            {
                throw new ArgumentException("Answer not found");
            }

            if (answer.UserId != userId)
            {
                throw new UnauthorizedAccessException("Only the answer author can edit this answer");
            }

            answer.Body = updateAnswerDto.Body;
            answer.UpdatedAt = DateTime.UtcNow;

            var updatedAnswer = await _answerRepository.UpdateAsync(answer);

            // Get the user information to populate the username
            var user = await _userRepository.GetByIdAsync(userId);

            return new AnswerDto
            {
                Id = updatedAnswer.Id,
                Body = updatedAnswer.Body,
                QuestionId = updatedAnswer.QuestionId,
                UserId = updatedAnswer.UserId,
                Username = user?.Username ?? "Unknown User",
                CreatedAt = updatedAnswer.CreatedAt,
                UpdatedAt = updatedAnswer.UpdatedAt,
                Votes = updatedAnswer.VoteCount,
                IsAccepted = updatedAnswer.IsAccepted
            };
        }

        public async Task DeleteAsync(int id, int userId)
        {
            var answer = await _answerRepository.GetByIdAsync(id);
            if (answer == null)
            {
                throw new ArgumentException("Answer not found");
            }

            if (answer.UserId != userId)
            {
                throw new UnauthorizedAccessException("Only the answer author can delete this answer");
            }

            await _answerRepository.DeleteAsync(id);

            // Deduct reputation from the user for deleting an answer
            await _userRepository.UpdateReputationAsync(userId, -1);
        }

        public async Task<AnswerDto> MarkAsAcceptedAsync(int answerId, int userId)
        {
            var answer = await _answerRepository.GetByIdAsync(answerId);
            if (answer == null)
            {
                throw new ArgumentException("Answer not found");
            }

            // Verify that the user is the question author
            var question = await _questionRepository.GetByIdAsync(answer.QuestionId);
            if (question == null || question.UserId != userId)
            {
                throw new UnauthorizedAccessException("Only the question author can accept answers");
            }

            await _answerRepository.MarkAsAcceptedAsync(answerId, answer.QuestionId);

            // Award reputation to the answer author
            await _userRepository.UpdateReputationAsync(answer.UserId, 15);

            // Get the updated answer
            var updatedAnswer = await _answerRepository.GetByIdAsync(answerId);
            
            return new AnswerDto
            {
                Id = updatedAnswer!.Id,
                Body = updatedAnswer.Body,
                QuestionId = updatedAnswer.QuestionId,
                UserId = updatedAnswer.UserId,
                CreatedAt = updatedAnswer.CreatedAt,
                UpdatedAt = updatedAnswer.UpdatedAt,
                Votes = updatedAnswer.VoteCount,
                IsAccepted = updatedAnswer.IsAccepted
            };
        }
    }
}
