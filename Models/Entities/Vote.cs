using StackOverFlowClone.Models.Entities;
using StackOverFlowClone.Models.Enum;

public class Vote
{
    public int Id { get; set; }
    public VoteType VoteType { get; set; }
    public TargetType TargetType { get; set; }

    // For Questions
    public int? QuestionId { get; set; }
    public Question? Question { get; set; }

    // For Answers
    public int? AnswerId { get; set; }
    public Answer? Answer { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }
}