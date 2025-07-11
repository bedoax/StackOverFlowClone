using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StackOverFlowClone.Data;
using StackOverFlowClone.Models.Entities;
using StackOverFlowClone.Services.Interfaces;

namespace StackOverFlowClone.Services.Implementations
{
    public class NotificationService: INotificationService
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;
        public NotificationService(AppDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }
        public async Task SendNotificationAsync(Notification notification)
        {
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            await _hubContext.Clients
                .Group($"user_{notification.UserId}")
                .SendAsync("ReceiveNotification", new
                {
                    notification.Title,
                    notification.Message,
                    notification.Type,
                    notification.TargetId,
                    notification.CreatedAt,
                });
        }
        public async Task MarkAsReadAsync(int notificationId, string userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId.ToString() == userId);

            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            int uid = int.Parse(userId);
            return await _context.Notifications
                .Where(n => !n.IsRead && n.UserId == uid)
                .CountAsync();
        }
    }
}
