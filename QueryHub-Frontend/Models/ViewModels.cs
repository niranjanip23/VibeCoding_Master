using System.ComponentModel.DataAnnotations;

namespace QueryHub_Frontend.Models
{
    public class QuestionViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public List<string> Tags { get; set; } = new List<string>();
        public string Author { get; set; } = "";
        public string UserName => Author; // Alias for compatibility
        public DateTime CreatedAt { get; set; }
        public DateTime CreatedDate => CreatedAt; // Alias for compatibility
        public int Votes { get; set; }
        public int AnswerVotes { get; set; } = 0; // Total votes from all answers
        public int Answers { get; set; }
        public int AnswerCount => Answers; // Alias for compatibility
        public int Views { get; set; }
        public bool IsAnswered => Answers > 0; // Computed property
        public List<string> TagList => Tags; // Alias for compatibility
    }

    public class QuestionDetailViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public List<string> Tags { get; set; } = new List<string>();
        public string Author { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public int Votes { get; set; }
        public int Views { get; set; }
        public List<AnswerViewModel> Answers { get; set; } = new List<AnswerViewModel>();
        public List<string> TagList => Tags; // Alias for compatibility
    }

    public class AnswerViewModel
    {
        public int Id { get; set; }
        public string Content { get; set; } = "";
        public string Author { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public int Votes { get; set; }
        public int QuestionId { get; set; }
        
        // Additional properties for compatibility with views
        public bool IsAccepted { get; set; } = false;
        public string UserName => Author; // Alias for compatibility
        public string UserAvatar { get; set; } = ""; // Not implemented in backend yet
        public DateTime CreatedDate => CreatedAt; // Alias for compatibility
        public List<object> Comments { get; set; } = new List<object>(); // Placeholder for comments
    }

    public class CreateQuestionViewModel
    {
        [Required]
        [StringLength(200)]
        [Display(Name = "Question Title")]
        public string Title { get; set; } = "";
        
        [Required]
        [Display(Name = "Question Description")]
        public string Description { get; set; } = "";
        
        [Display(Name = "Tags")]
        public string Tags { get; set; } = "";
    }
}
