using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StackOverFlowClone.Data;
using StackOverFlowClone.Models.Entities;
using StackOverFlowClone.Models.Enum;
using System;
using System.Text.RegularExpressions;
/*using YourApp.Data;
using YourApp.Hubs;
using YourApp.Models;*/

public class MentionService : IMentionService
{
    private readonly AppDbContext _context;
    private readonly IHubContext<NotificationHub> _notificationHub;


public MentionService(AppDbContext context, IHubContext<NotificationHub> notificationHub)
    {
        _context = context;
        _notificationHub = notificationHub;
    }

    public async Task<List<int>> HandleMentionsAsync(string content)
    {
        var mentionedUsernames = ExtractMentions(content); // مثلًا ترجع ["jack", "dalia"]

        var userIds = await _context.Users
            .Where(u => mentionedUsernames.Contains(u.UserName))
            .Select(u => u.Id)
            .ToListAsync();

        // ممكن تسجل الـ mentions في جدول لو حابب

        return userIds;
    }
    private List<string> ExtractMentions(string content)
    {
        // استخدم Regex لاستخراج mentions
        var mentionPattern = @"@(\w+)";
        var matches = Regex.Matches(content, mentionPattern);
        var mentionedUsernames = new List<string>();
        foreach (Match match in matches)
        {
            if (match.Groups.Count > 1)
            {
                var username = match.Groups[1].Value;
                mentionedUsernames.Add(username);
            }
        }
        return mentionedUsernames;
    }
}