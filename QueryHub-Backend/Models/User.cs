using System.ComponentModel.DataAnnotations;

namespace QueryHub_Backend.Models
{
    public class User
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string Department { get; set; } = string.Empty;
        
        public string? Avatar { get; set; }
        
        public int Reputation { get; set; } = 0;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
        public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();
        public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
