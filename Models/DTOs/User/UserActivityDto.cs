namespace StackOverFlowClone.Models.DTOs.User
{
    public class UserActivityDto
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;

        public int QuestionCount { get; set; }
        public int AnswerCount { get; set; }
        public int TotalVotes { get; set; }
        public int Reputation { get; set; }

        // Optional: Uncomment below if you want to show the actual questions/answers
        // public ICollection<QuestionDto>? Questions { get; set; }
        // public ICollection<AnswerDto>? Answers { get; set; }
    }
}
