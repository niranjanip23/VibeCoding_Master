using System.ComponentModel.DataAnnotations;

namespace QueryHub_Backend.Models
{
    public enum CommentTargetType
    {
        Question = 1,
        Answer = 2
    }
    
    public class Comment
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(1000)]
        public string Content { get; set; } = string.Empty;
        
        [Required]
        [StringLength(1000)]
        public string Body { get; set; } = string.Empty;
        
        public int UserId { get; set; }
        
        public int TargetId { get; set; } // QuestionId or AnswerId
        
        public int? QuestionId { get; set; } // For question comments
        
        public int? AnswerId { get; set; } // For answer comments
        
        public CommentTargetType TargetType { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public virtual User User { get; set; } = null!;
    }
}
