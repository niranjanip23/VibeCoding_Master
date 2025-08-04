using System.ComponentModel.DataAnnotations;

namespace QueryHub_Backend.Models
{
    public class Question
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(500)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public string Body { get; set; } = string.Empty;
        
        public int UserId { get; set; }
        
        public int ViewCount { get; set; } = 0;
        
        public int Views { get; set; } = 0;
        
        public int VoteCount { get; set; } = 0;
        
        public int AnswerCount { get; set; } = 0;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();
        public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<QuestionTag> QuestionTags { get; set; } = new List<QuestionTag>();
    }
}
