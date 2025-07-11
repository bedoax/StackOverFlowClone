using StackOverFlowClone.Models.Entities;

namespace StackOverFlowClone.Services.Interfaces
{
    public interface INotificationService
    {
        Task SendNotificationAsync(Notification notification);
        Task<int> GetUnreadCountAsync(string userId);
        Task MarkAsReadAsync(int notificationId, string userId);
    }
}
