using System.ComponentModel.DataAnnotations;
using QueryHub_Backend.Models;

namespace QueryHub_Backend.DTOs
{
    public class VoteQuestionDto
    {
        [Required]
        public int QuestionId { get; set; }
        
        [Required]
        public VoteType VoteType { get; set; } // 1 = Upvote, 2 = Downvote
    }
    
    public class VoteAnswerDto
    {
        [Required]
        public int AnswerId { get; set; }
        
        [Required]
        public VoteType VoteType { get; set; } // 1 = Upvote, 2 = Downvote
    }
    
    public class VoteDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? QuestionId { get; set; }
        public int? AnswerId { get; set; }
        public VoteType VoteType { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRemoved { get; set; }
    }
}
