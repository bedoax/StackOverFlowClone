namespace StackOverFlowClone.Models.DTOs.Question
{
    public class QuestionDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public int UserId { get; set; }

        // Enriched fields for response
        public string? UserName { get; set; }
        public int? VoteCount { get; set; }
        public int AnswerCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public ICollection<string> Tags { get; set; } 
    }
}
