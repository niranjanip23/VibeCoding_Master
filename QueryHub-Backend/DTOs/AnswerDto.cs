using System.ComponentModel.DataAnnotations;

namespace QueryHub_Backend.DTOs
{
    public class CreateAnswerDto
    {
        [Required]
        [StringLength(10000, MinimumLength = 10)]
        public string Body { get; set; } = string.Empty;
        
        [Required]
        public int QuestionId { get; set; }
    }
    
    public class UpdateAnswerDto
    {
        [Required]
        [StringLength(10000, MinimumLength = 10)]
        public string Body { get; set; } = string.Empty;
    }
    
    public class AnswerDto
    {
        public int Id { get; set; }
        public string Body { get; set; } = string.Empty;
        public int QuestionId { get; set; }
        public int UserId { get; set; }
        public int Votes { get; set; }
        public bool IsAccepted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
