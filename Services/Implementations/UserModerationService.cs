using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StackOverFlowClone.Data;
using StackOverFlowClone.Models.Entities;
using StackOverFlowClone.Models.DTOs.User;
using StackOverFlowClone.Services.Interfaces;
using StackOverFlowClone.Models.Role;

public class UserModerationService : IUserModerationService
{
    private readonly AppDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;

    public UserModerationService(AppDbContext context, UserManager<User> userManager, RoleManager<Role> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<bool> BanUserAsync(int userId, string reason = null, DateTime? bannedUntil = null)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        user.IsBanned = true;
        user.BanReason = reason;
        user.BannedUntil = bannedUntil;

        // Add to Banned role and remove from other roles if needed
        if (await _roleManager.RoleExistsAsync("Banned"))
        {
            await _userManager.AddToRoleAsync(user, "Banned");
            await _userManager.RemoveFromRoleAsync(user, "Moderator");  // Remove moderator if banned
        }

        await _context.SaveChangesAsync();
        return true;
    }

    // StackOverFlowClone.Services.Implementations/UserModerationService.cs
    public async Task<bool> UnbanUserAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        user.IsBanned = false;
        user.BanReason = null;
        user.BannedUntil = null;  // Correctly unban by setting to null

        // Remove from Banned role
        if (await _userManager.IsInRoleAsync(user, "Banned"))
        {
            await _userManager.RemoveFromRoleAsync(user, "Banned");
        }

        // Add to User role if not already assigned
        if (!await _userManager.IsInRoleAsync(user, "User"))
        {
            await _userManager.AddToRoleAsync(user, "User");
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> PromoteToModeratorAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null || user.IsBanned) return false;

        // Add to Moderator role
        if (await _roleManager.RoleExistsAsync("Moderator"))
        {
            await _userManager.AddToRoleAsync(user, "Moderator");
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DemoteFromModeratorAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        // Remove from Moderator role
        await _userManager.RemoveFromRoleAsync(user, "Moderator");

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<UserActivityDto> GetUserActivityAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return null;

        var questionCount = await _context.Questions.CountAsync(q => q.UserId == userId);
        var answerCount = await _context.Answers.CountAsync(a => a.UserId == userId);
        var votesCast = await _context.Votes.CountAsync(v => v.UserId == userId);

        return new UserActivityDto
        {
            Id = user.Id,
            UserName = user.UserName,
            QuestionCount = questionCount,
            AnswerCount = answerCount,
            TotalVotes = votesCast,
            Reputation = await GetReputationAsync(userId)
        };
    }

    public async Task<int> GetReputationAsync(int userId)
    {
        // Calculate reputation based on votes received on user's content
        var questionUpvotes = await _context.Votes
            .Join(_context.Questions, v => v.QuestionId, q => q.Id, (v, q) => new { Vote = v, Question = q })
            .Where(x => x.Question.UserId == userId && x.Vote.TargetType == TargetType.Question && x.Vote.VoteType == VoteType.UpVote)
            .CountAsync();

        var questionDownvotes = await _context.Votes
            .Join(_context.Questions, v => v.QuestionId, q => q.Id, (v, q) => new { Vote = v, Question = q })
            .Where(x => x.Question.UserId == userId && x.Vote.TargetType == TargetType.Question && x.Vote.VoteType == VoteType.DownVote)
            .CountAsync();

        var answerUpvotes = await _context.Votes
            .Join(_context.Answers, v => v.AnswerId, a => a.Id, (v, a) => new { Vote = v, Answer = a })
            .Where(x => x.Answer.UserId == userId && x.Vote.TargetType == TargetType.Answer && x.Vote.VoteType == VoteType.UpVote)
            .CountAsync();

        var answerDownvotes = await _context.Votes
            .Join(_context.Answers, v => v.AnswerId, a => a.Id, (v, a) => new { Vote = v, Answer = a })
            .Where(x => x.Answer.UserId == userId && x.Vote.TargetType == TargetType.Answer && x.Vote.VoteType == VoteType.DownVote)
            .CountAsync();

        // StackOverflow-like reputation calculation
        // +10 for question upvote, -2 for question downvote
        // +10 for answer upvote, -2 for answer downvote
        var reputation = (questionUpvotes * 10) - (questionDownvotes * 2) +
                        (answerUpvotes * 10) - (answerDownvotes * 2);

        // Update user's reputation in database
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            user.Reputation = Math.Max(0, reputation); // Don't allow negative reputation
            await _context.SaveChangesAsync();
        }

        return Math.Max(0, reputation);
    }

    public async Task<bool> IsUserBannedAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

/*        // Check if ban has expired
        if (user.IsBanned && user.BannedUntil.HasValue && user.BannedUntil.Value <= DateTime.UtcNow)
        {
            await UnbanUserAsync(userId);
            return false;
        }*/

        return user.IsBanned;
    }
}