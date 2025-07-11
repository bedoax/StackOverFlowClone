using Microsoft.AspNetCore.SignalR;
using StackOverFlowClone.Services.Interfaces;
using System.Text.RegularExpressions;

public class NotificationHub : Hub
{
    private readonly INotificationService _notificationService;

    public NotificationHub(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task MarkAsRead(int notificationId)
    {
        var userId = Context.UserIdentifier;
        if (userId != null)
        {
            await _notificationService.MarkAsReadAsync(notificationId, userId);
        }
    }

    public async Task<int> LoadUnreadCount()
    {
        var userId = Context.UserIdentifier;
        if (userId != null)
        {
            return await _notificationService.GetUnreadCountAsync(userId);
        }
        return 0;
    }

    public Task Ping()
    {
        return Clients.Caller.SendAsync("Pong");
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (userId != null)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            await Clients.Caller.SendAsync("Welcome", "You are connected to the notification hub.");
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}
