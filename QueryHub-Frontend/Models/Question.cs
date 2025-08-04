using System.ComponentModel.DataAnnotations;

namespace QueryHub_Frontend.Models
{
    public class Question
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        [Display(Name = "Question Title")]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Question Description")]
        public string Description { get; set; } = string.Empty;
        
        [Display(Name = "Tags (comma separated)")]
        public string Tags { get; set; } = string.Empty;
        
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserAvatar { get; set; } = string.Empty;
        
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        
        public int Views { get; set; }
        public int Votes { get; set; }
        public int AnswerCount { get; set; }
        
        public bool IsAnswered { get; set; }
        
        public List<Answer> Answers { get; set; } = new List<Answer>();
        public List<Comment> Comments { get; set; } = new List<Comment>();
        
        public List<string> TagList => Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.Trim()).Where(t => !string.IsNullOrEmpty(t)).ToList();
    }

    public class QuestionListViewModel
    {
        public List<QuestionViewModel> Questions { get; set; } = new List<QuestionViewModel>();
        public string SearchQuery { get; set; } = string.Empty;
        public string SelectedTag { get; set; } = string.Empty;
        public List<string> PopularTags { get; set; } = new List<string>();
        public int TotalQuestions { get; set; }
        public int TotalAnswers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalTags { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages => (int)Math.Ceiling((double)TotalQuestions / PageSize);
    }

    public class DashboardStatistics
    {
        public int TotalQuestions { get; set; }
        public int TotalAnswers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalTags { get; set; }
    }
}
