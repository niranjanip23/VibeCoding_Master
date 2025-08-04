namespace QueryHub_Backend.Models
{
    public enum VoteType
    {
        Upvote = 1,
        Downvote = -1
    }
    
    public enum VoteTargetType
    {
        Question = 1,
        Answer = 2
    }
    
    public class Vote
    {
        public int Id { get; set; }
        
        public int UserId { get; set; }
        
        public int TargetId { get; set; } // QuestionId or AnswerId
        
        public int? QuestionId { get; set; } // For question votes
        
        public int? AnswerId { get; set; } // For answer votes
        
        public VoteTargetType TargetType { get; set; }
        
        public VoteType VoteType { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual User User { get; set; } = null!;
    }
}
