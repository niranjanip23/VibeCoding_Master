using QueryHub_Backend.DTOs;
using QueryHub_Backend.Interfaces;
using QueryHub_Backend.Models;

namespace QueryHub_Backend.Services
{
    public class VoteService : IVoteService
    {
        private readonly IVoteRepository _voteRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly IAnswerRepository _answerRepository;
        private readonly IUserRepository _userRepository;

        public VoteService(
            IVoteRepository voteRepository,
            IQuestionRepository questionRepository,
            IAnswerRepository answerRepository,
            IUserRepository userRepository)
        {
            _voteRepository = voteRepository;
            _questionRepository = questionRepository;
            _answerRepository = answerRepository;
            _userRepository = userRepository;
        }

        public async Task<VoteDto> VoteOnQuestionAsync(VoteQuestionDto voteDto, int userId)
        {
            // Check if user has already voted on this question
            var existingVote = await _voteRepository.GetByUserAndQuestionAsync(userId, voteDto.QuestionId);
            
            Vote vote;
            int reputationChange = 0;
            int voteChange = 0;

            if (existingVote != null)
            {
                // User has already voted, update the vote
                var oldVoteType = existingVote.VoteType;
                
                if (oldVoteType == voteDto.VoteType)
                {
                    // Same vote type, remove the vote
                    await _voteRepository.DeleteAsync(existingVote.Id);
                    voteChange = oldVoteType == VoteType.Upvote ? -1 : 1;
                    reputationChange = oldVoteType == VoteType.Upvote ? -10 : 2;
                    
                    // Return a DTO indicating the vote was removed
                    return new VoteDto
                    {
                        Id = 0,
                        UserId = userId,
                        QuestionId = voteDto.QuestionId,
                        VoteType = VoteType.Upvote, // This won't be used since Id is 0
                        CreatedAt = DateTime.UtcNow,
                        IsRemoved = true
                    };
                }
                else
                {
                    // Different vote type, update the vote
                    existingVote.VoteType = voteDto.VoteType;
                    vote = await _voteRepository.UpdateAsync(existingVote);
                    
                    // Calculate changes (from old vote to new vote)
                    voteChange = oldVoteType == VoteType.Upvote ? -2 : 2; // -1 to +1 = +2, +1 to -1 = -2
                    reputationChange = oldVoteType == VoteType.Upvote ? -12 : 12; // Remove old +10/-2, add new +10/-2
                }
            }
            else
            {
                // New vote
                vote = new Vote
                {
                    UserId = userId,
                    QuestionId = voteDto.QuestionId,
                    VoteType = voteDto.VoteType,
                    CreatedAt = DateTime.UtcNow
                };
                
                vote = await _voteRepository.CreateAsync(vote);
                voteChange = voteDto.VoteType == VoteType.Upvote ? 1 : -1;
                reputationChange = voteDto.VoteType == VoteType.Upvote ? 10 : -2;
            }

            // Update question vote count
            await _questionRepository.UpdateVotesAsync(voteDto.QuestionId, voteChange);

            // Update question author's reputation
            var question = await _questionRepository.GetByIdAsync(voteDto.QuestionId);
            if (question != null)
            {
                await _userRepository.UpdateReputationAsync(question.UserId, reputationChange);
            }

            return new VoteDto
            {
                Id = vote.Id,
                UserId = vote.UserId,
                QuestionId = vote.QuestionId,
                VoteType = vote.VoteType,
                CreatedAt = vote.CreatedAt,
                IsRemoved = false
            };
        }

        public async Task<VoteDto> VoteOnAnswerAsync(VoteAnswerDto voteDto, int userId)
        {
            // Check if user has already voted on this answer
            var existingVote = await _voteRepository.GetByUserAndAnswerAsync(userId, voteDto.AnswerId);
            
            Vote vote;
            int reputationChange = 0;
            int voteChange = 0;

            if (existingVote != null)
            {
                // User has already voted, update the vote
                var oldVoteType = existingVote.VoteType;
                
                if (oldVoteType == voteDto.VoteType)
                {
                    // Same vote type, remove the vote
                    await _voteRepository.DeleteAsync(existingVote.Id);
                    voteChange = oldVoteType == VoteType.Upvote ? -1 : 1;
                    reputationChange = oldVoteType == VoteType.Upvote ? -10 : 2;
                    
                    // Return a DTO indicating the vote was removed
                    return new VoteDto
                    {
                        Id = 0,
                        UserId = userId,
                        AnswerId = voteDto.AnswerId,
                        VoteType = VoteType.Upvote, // This won't be used since Id is 0
                        CreatedAt = DateTime.UtcNow,
                        IsRemoved = true
                    };
                }
                else
                {
                    // Different vote type, update the vote
                    existingVote.VoteType = voteDto.VoteType;
                    vote = await _voteRepository.UpdateAsync(existingVote);
                    
                    // Calculate changes (from old vote to new vote)
                    voteChange = oldVoteType == VoteType.Upvote ? -2 : 2;
                    reputationChange = oldVoteType == VoteType.Upvote ? -12 : 12;
                }
            }
            else
            {
                // New vote - need to get the question ID for the answer
                var answer = await _answerRepository.GetByIdAsync(voteDto.AnswerId);
                if (answer == null)
                {
                    throw new ArgumentException("Answer not found");
                }

                vote = new Vote
                {
                    UserId = userId,
                    QuestionId = answer.QuestionId,
                    AnswerId = voteDto.AnswerId,
                    VoteType = voteDto.VoteType,
                    CreatedAt = DateTime.UtcNow
                };
                
                vote = await _voteRepository.CreateAsync(vote);
                voteChange = voteDto.VoteType == VoteType.Upvote ? 1 : -1;
                reputationChange = voteDto.VoteType == VoteType.Upvote ? 10 : -2;
            }

            // Update answer vote count
            await _answerRepository.UpdateVotesAsync(voteDto.AnswerId, voteChange);

            // Update answer author's reputation
            var answerForReputation = await _answerRepository.GetByIdAsync(voteDto.AnswerId);
            if (answerForReputation != null)
            {
                await _userRepository.UpdateReputationAsync(answerForReputation.UserId, reputationChange);
            }

            return new VoteDto
            {
                Id = vote.Id,
                UserId = vote.UserId,
                QuestionId = vote.QuestionId,
                AnswerId = vote.AnswerId,
                VoteType = vote.VoteType,
                CreatedAt = vote.CreatedAt,
                IsRemoved = false
            };
        }

        public async Task<IEnumerable<VoteDto>> GetUserVotesAsync(int userId)
        {
            var votes = await _voteRepository.GetByUserIdAsync(userId);
            return votes.Select(vote => new VoteDto
            {
                Id = vote.Id,
                UserId = vote.UserId,
                QuestionId = vote.QuestionId,
                AnswerId = vote.AnswerId,
                VoteType = vote.VoteType,
                CreatedAt = vote.CreatedAt,
                IsRemoved = false
            });
        }

        public async Task<int> GetQuestionVoteCountAsync(int questionId)
        {
            return await _voteRepository.GetQuestionVoteCountAsync(questionId);
        }

        public async Task<int> GetAnswerVoteCountAsync(int answerId)
        {
            return await _voteRepository.GetAnswerVoteCountAsync(answerId);
        }

        public async Task<VoteDto?> GetUserVoteOnQuestionAsync(int userId, int questionId)
        {
            var vote = await _voteRepository.GetByUserAndQuestionAsync(userId, questionId);
            if (vote == null) return null;

            return new VoteDto
            {
                Id = vote.Id,
                UserId = vote.UserId,
                QuestionId = vote.QuestionId,
                VoteType = vote.VoteType,
                CreatedAt = vote.CreatedAt,
                IsRemoved = false
            };
        }

        public async Task<VoteDto?> GetUserVoteOnAnswerAsync(int userId, int answerId)
        {
            var vote = await _voteRepository.GetByUserAndAnswerAsync(userId, answerId);
            if (vote == null) return null;

            return new VoteDto
            {
                Id = vote.Id,
                UserId = vote.UserId,
                QuestionId = vote.QuestionId,
                AnswerId = vote.AnswerId,
                VoteType = vote.VoteType,
                CreatedAt = vote.CreatedAt,
                IsRemoved = false
            };
        }
    }
}
