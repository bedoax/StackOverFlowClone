using StackOverFlowClone.Models.DTOs.Answer;

namespace StackOverFlowClone.Services.Interfaces
{
    public interface IAnswerService
    {
        Task<AnswerDto> CreateAnswerAsync(int questionId, CreateAnswerDto answerDto, int userId);
        Task<bool> UpdateAnswerAsync(int answerId, UpdateAnswerDto answerDto);
        Task<bool> DeleteAnswerAsync(int answerId, int userId);
        Task<bool> VoteAnswerAsync(int answerId, int userId, bool isUpvote);
        Task<IEnumerable<AnswerDto>> GetAnswerForQuestionAsync(int questionId, int pageNumber, int size);
        Task<int> GetAnswerVoteCountAsync(int answerId);
    }
}
