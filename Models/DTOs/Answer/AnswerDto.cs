namespace StackOverFlowClone.Models.DTOs.Answer
{
    public class AnswerDto
    {
        public int Id { get; set; }
        public string Body { get; set; } = string.Empty;
        public int QuestionId { get; set; }
        public int UserId { get; set; }

        // Enriched fields
        public string? UserName { get; set; }
        public int? VoteCount { get; set; }
    }
}
