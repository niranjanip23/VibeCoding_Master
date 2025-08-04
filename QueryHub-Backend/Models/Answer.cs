using System.ComponentModel.DataAnnotations;

namespace QueryHub_Backend.Models
{
    public class Answer
    {
        public int Id { get; set; }
        
        [Required]
        public string Content { get; set; } = string.Empty;
        
        [Required]
        public string Body { get; set; } = string.Empty;
        
        public int QuestionId { get; set; }
        
        public int UserId { get; set; }
        
        public int VoteCount { get; set; } = 0;
        
        public bool IsAccepted { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public virtual Question Question { get; set; } = null!;
        public virtual User User { get; set; } = null!;
        public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
