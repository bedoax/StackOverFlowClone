using StackOverFlowClone.Models.DTOs.User;

namespace StackOverFlowClone.Services.Interfaces
{
    public interface IUserModerationService
    {
        // Ban/Unban operations
        Task<bool> BanUserAsync(int userId, string reason = null, DateTime? bannedUntil = null);
        Task<bool> UnbanUserAsync(int userId);
        Task<bool> IsUserBannedAsync(int userId);

        // Role management
        Task<bool> PromoteToModeratorAsync(int userId);
        Task<bool> DemoteFromModeratorAsync(int userId);

        // User activity and statistics
        Task<UserActivityDto> GetUserActivityAsync(int userId);
        Task<int> GetReputationAsync(int userId);
    }
}