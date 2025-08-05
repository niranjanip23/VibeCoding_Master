namespace QueryHub_Frontend.Models
{
    public class QuestionApiModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Body { get; set; } = "";
        public int UserId { get; set; }
        public string Username { get; set; } = "";
        public int Views { get; set; }
        public int Votes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public List<AnswerApiModel>? Answers { get; set; } = new List<AnswerApiModel>();
    }

    public class QuestionDetailApiModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Body { get; set; } = "";
        public int UserId { get; set; }
        public string Username { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int Votes { get; set; }
        public int Views { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public List<AnswerApiModel>? Answers { get; set; }
    }

    public class AnswerApiModel
    {
        public int Id { get; set; }
        public string Body { get; set; } = "";
        public string Username { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public int VoteCount { get; set; }
        public int QuestionId { get; set; }
        public bool IsAccepted { get; set; } = false;
    }

    public class TagApiModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
    }
}
