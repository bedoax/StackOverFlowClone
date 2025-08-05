namespace StackOverFlowClone.Models.DTOs.Answer
{
    public class AnswerFromChatGpt
    {
        public int QuestionId { get; set; }

        public string Body { get; set; } = string.Empty;
        public int UserId { get; set; }
    }
}
