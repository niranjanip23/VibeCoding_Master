using System.ComponentModel.DataAnnotations;

namespace QueryHub_Backend.DTOs
{
    public class CreateQuestionDto
    {
        [Required]
        [StringLength(500, MinimumLength = 10)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        [StringLength(10000, MinimumLength = 20)]
        public string Body { get; set; } = string.Empty;
        
        public List<string>? Tags { get; set; } = new List<string>();
    }
    
    public class UpdateQuestionDto
    {
        [Required]
        [StringLength(500, MinimumLength = 10)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        [StringLength(10000, MinimumLength = 20)]
        public string Body { get; set; } = string.Empty;
        
        public List<string>? Tags { get; set; } = new List<string>();
    }
    
    public class QuestionDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public int Views { get; set; }
        public int Votes { get; set; }
        public int AnswerVotes { get; set; } = 0; // Total votes from all answers
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public List<AnswerDto> Answers { get; set; } = new List<AnswerDto>();
    }
}
