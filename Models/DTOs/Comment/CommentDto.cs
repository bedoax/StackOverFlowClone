using StackOverFlowClone.Models.Enum;

namespace StackOverFlowClone.Models.DTOs.Comment
{
    public class CommentDto
    {
        public int Id { get; set; }
        public string Body { get; set; } = string.Empty;
        public TargetType TargetType { get; set; }   // Question or Answer
        /*public int TargetId { get; set; } */
        // Will refer to QuestionId or AnswerId based on TargetType
        public int questionId { get; set; }
        public int answerId { get; set; }
        public int UserId { get; set; }
    }
}
