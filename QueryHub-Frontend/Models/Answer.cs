using System.ComponentModel.DataAnnotations;

namespace QueryHub_Frontend.Models
{
    public class Answer
    {
        public int Id { get; set; }
        
        [Required]
        [Display(Name = "Your Answer")]
        public string Content { get; set; } = string.Empty;
        
        public int QuestionId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserAvatar { get; set; } = string.Empty;
        
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        
        public int Votes { get; set; }
        public bool IsAccepted { get; set; }
        
        public List<Comment> Comments { get; set; } = new List<Comment>();
    }

    public class Comment
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(500)]
        public string Content { get; set; } = string.Empty;
        
        public int? QuestionId { get; set; }
        public int? AnswerId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        
        public DateTime CreatedDate { get; set; }
    }
}
