using StackOverFlowClone.Models.Entities;

namespace StackOverFlowClone.Services.Interfaces
{
    public interface IVoteService
    {
        Task<int> GetVoteCountAsync(int targetId, TargetType targetType);
        Task<bool> VoteAsync(int targetId, TargetType targetType, int userId, bool isUpvote);
        Task<bool> UndoVoteAsync(int targetId, TargetType targetType, int userId);
        Task<int?> GetUserVoteAsync(int targetId, TargetType targetType, int userId);
    }
}