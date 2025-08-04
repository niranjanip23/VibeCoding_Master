using System.ComponentModel.DataAnnotations;

namespace QueryHub_Backend.Models
{
    public class Tag
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        public int UsageCount { get; set; } = 0;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual ICollection<QuestionTag> QuestionTags { get; set; } = new List<QuestionTag>();
    }
    
    public class QuestionTag
    {
        public int QuestionId { get; set; }
        public int TagId { get; set; }
        
        // Navigation properties
        public virtual Question Question { get; set; } = null!;
        public virtual Tag Tag { get; set; } = null!;
    }
}
