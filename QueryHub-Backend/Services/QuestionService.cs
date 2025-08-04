using QueryHub_Backend.DTOs;
using QueryHub_Backend.Interfaces;
using QueryHub_Backend.Models;

namespace QueryHub_Backend.Services
{
    public class QuestionService : IQuestionService
    {
        private readonly IQuestionRepository _questionRepository;
        private readonly ITagRepository _tagRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAnswerService _answerService;

        public QuestionService(
            IQuestionRepository questionRepository,
            ITagRepository tagRepository,
            IUserRepository userRepository,
            IAnswerService answerService)
        {
            _questionRepository = questionRepository;
            _tagRepository = tagRepository;
            _userRepository = userRepository;
            _answerService = answerService;
        }

        public async Task<QuestionDto?> GetByIdAsync(int id)
        {
            var question = await _questionRepository.GetByIdAsync(id);
            if (question == null) return null;

            await _questionRepository.IncrementViewsAsync(id);
            
            var tags = await _tagRepository.GetByQuestionIdAsync(id);
            var answers = await _answerService.GetByQuestionIdAsync(id);
            
            return new QuestionDto
            {
                Id = question.Id,
                Title = question.Title,
                Body = question.Body,
                UserId = question.UserId,
                CreatedAt = question.CreatedAt,
                UpdatedAt = question.UpdatedAt,
                Views = question.Views + 1, // Include the incremented view
                Votes = question.VoteCount,
                Tags = tags.Select(t => t.Name).ToList(),
                Answers = answers.ToList()
            };
        }

        public async Task<IEnumerable<QuestionDto>> GetAllAsync()
        {
            var questions = await _questionRepository.GetAllAsync();
            var questionDtos = new List<QuestionDto>();

            foreach (var question in questions)
            {
                var tags = await _tagRepository.GetByQuestionIdAsync(question.Id);
                var answers = await _answerService.GetByQuestionIdAsync(question.Id);
                
                questionDtos.Add(new QuestionDto
                {
                    Id = question.Id,
                    Title = question.Title,
                    Body = question.Body,
                    UserId = question.UserId,
                    CreatedAt = question.CreatedAt,
                    UpdatedAt = question.UpdatedAt,
                    Views = question.Views,
                    Votes = question.VoteCount,
                    Tags = tags.Select(t => t.Name).ToList(),
                    Answers = answers.ToList()
                });
            }

            return questionDtos;
        }

        public async Task<IEnumerable<QuestionDto>> GetByUserIdAsync(int userId)
        {
            var questions = await _questionRepository.GetByUserIdAsync(userId);
            var questionDtos = new List<QuestionDto>();

            foreach (var question in questions)
            {
                var tags = await _tagRepository.GetByQuestionIdAsync(question.Id);
                var answers = await _answerService.GetByQuestionIdAsync(question.Id);
                
                questionDtos.Add(new QuestionDto
                {
                    Id = question.Id,
                    Title = question.Title,
                    Body = question.Body,
                    UserId = question.UserId,
                    CreatedAt = question.CreatedAt,
                    UpdatedAt = question.UpdatedAt,
                    Views = question.Views,
                    Votes = question.VoteCount,
                    Tags = tags.Select(t => t.Name).ToList(),
                    Answers = answers.ToList()
                });
            }

            return questionDtos;
        }

        public async Task<IEnumerable<QuestionDto>> SearchAsync(string searchTerm)
        {
            var questions = await _questionRepository.SearchAsync(searchTerm);
            var questionDtos = new List<QuestionDto>();

            foreach (var question in questions)
            {
                var tags = await _tagRepository.GetByQuestionIdAsync(question.Id);
                var answers = await _answerService.GetByQuestionIdAsync(question.Id);
                
                questionDtos.Add(new QuestionDto
                {
                    Id = question.Id,
                    Title = question.Title,
                    Body = question.Body,
                    UserId = question.UserId,
                    CreatedAt = question.CreatedAt,
                    UpdatedAt = question.UpdatedAt,
                    Views = question.Views,
                    Votes = question.VoteCount,
                    Tags = tags.Select(t => t.Name).ToList(),
                    Answers = answers.ToList()
                });
            }

            return questionDtos;
        }

        public async Task<IEnumerable<QuestionDto>> GetByTagAsync(string tagName)
        {
            var questions = await _questionRepository.GetByTagAsync(tagName);
            var questionDtos = new List<QuestionDto>();

            foreach (var question in questions)
            {
                var tags = await _tagRepository.GetByQuestionIdAsync(question.Id);
                var answers = await _answerService.GetByQuestionIdAsync(question.Id);
                
                questionDtos.Add(new QuestionDto
                {
                    Id = question.Id,
                    Title = question.Title,
                    Body = question.Body,
                    UserId = question.UserId,
                    CreatedAt = question.CreatedAt,
                    UpdatedAt = question.UpdatedAt,
                    Views = question.Views,
                    Votes = question.VoteCount,
                    Tags = tags.Select(t => t.Name).ToList(),
                    Answers = answers.ToList()
                });
            }

            return questionDtos;
        }

        public async Task<QuestionDto> CreateAsync(CreateQuestionDto createQuestionDto, int userId)
        {
            var question = new Question
            {
                Title = createQuestionDto.Title,
                Body = createQuestionDto.Body,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Views = 0,
                VoteCount = 0
            };

            var createdQuestion = await _questionRepository.CreateAsync(question);

            // Handle tags
            var tagIds = new List<int>();
            if (createQuestionDto.Tags != null && createQuestionDto.Tags.Any())
            {
                foreach (var tagName in createQuestionDto.Tags)
                {
                    var tag = await _tagRepository.GetByNameAsync(tagName);
                    if (tag == null)
                    {
                        // Create new tag if it doesn't exist
                        tag = await _tagRepository.CreateAsync(new Tag
                        {
                            Name = tagName,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                    tagIds.Add(tag.Id);
                    await _tagRepository.AddTagToQuestionAsync(createdQuestion.Id, tag.Id);
                }
            }

            return new QuestionDto
            {
                Id = createdQuestion.Id,
                Title = createdQuestion.Title,
                Body = createdQuestion.Body,
                UserId = createdQuestion.UserId,
                CreatedAt = createdQuestion.CreatedAt,
                UpdatedAt = createdQuestion.UpdatedAt,
                Views = createdQuestion.Views,
                Votes = createdQuestion.VoteCount,
                Tags = createQuestionDto.Tags?.ToList() ?? new List<string>()
            };
        }

        public async Task<QuestionDto> UpdateAsync(int id, UpdateQuestionDto updateQuestionDto, int userId)
        {
            var question = await _questionRepository.GetByIdAsync(id);
            if (question == null)
            {
                throw new ArgumentException("Question not found");
            }

            if (question.UserId != userId)
            {
                throw new UnauthorizedAccessException("Only the question author can edit this question");
            }

            question.Title = updateQuestionDto.Title;
            question.Body = updateQuestionDto.Body;
            question.UpdatedAt = DateTime.UtcNow;

            var updatedQuestion = await _questionRepository.UpdateAsync(question);

            // Update tags if provided
            if (updateQuestionDto.Tags != null)
            {
                // Remove existing tags for this question
                var existingTags = await _tagRepository.GetByQuestionIdAsync(id);
                foreach (var existingTag in existingTags)
                {
                    await _tagRepository.RemoveTagFromQuestionAsync(id, existingTag.Id);
                }

                // Add new tags
                foreach (var tagName in updateQuestionDto.Tags)
                {
                    var tag = await _tagRepository.GetByNameAsync(tagName);
                    if (tag == null)
                    {
                        tag = await _tagRepository.CreateAsync(new Tag
                        {
                            Name = tagName,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                    await _tagRepository.AddTagToQuestionAsync(id, tag.Id);
                }
            }

            var tags = await _tagRepository.GetByQuestionIdAsync(id);
            return new QuestionDto
            {
                Id = updatedQuestion.Id,
                Title = updatedQuestion.Title,
                Body = updatedQuestion.Body,
                UserId = updatedQuestion.UserId,
                CreatedAt = updatedQuestion.CreatedAt,
                UpdatedAt = updatedQuestion.UpdatedAt,
                Views = updatedQuestion.Views,
                Votes = updatedQuestion.VoteCount,
                Tags = tags.Select(t => t.Name).ToList()
            };
        }

        public async Task DeleteAsync(int id, int userId)
        {
            var question = await _questionRepository.GetByIdAsync(id);
            if (question == null)
            {
                throw new ArgumentException("Question not found");
            }

            if (question.UserId != userId)
            {
                throw new UnauthorizedAccessException("Only the question author can delete this question");
            }

            await _questionRepository.DeleteAsync(id);
        }
    }
}
