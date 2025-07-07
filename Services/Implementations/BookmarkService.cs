using Microsoft.EntityFrameworkCore;
using StackOverFlowClone.Data;
using StackOverFlowClone.Models.DTOs.Bookmark;
using StackOverFlowClone.Models.Entities;
using StackOverFlowClone.Services.Interfaces;
using System.Security.Claims;

public class BookmarkService : IBookmarkService
{
    private readonly AppDbContext _context;

    public BookmarkService(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddBookmarkAsync(int userId, int questionId)
    {
        var exists = await _context.Bookmarks
            .AnyAsync(b => b.UserId == userId && b.QuestionId == questionId);

        if (!exists)
        {
            var bookmark = new Bookmark
            {
                UserId = userId,
                QuestionId = questionId
            };
            _context.Bookmarks.Add(bookmark);
            await _context.SaveChangesAsync();
        }
    }

    public async Task RemoveBookmarkAsync(int userId, int questionId)
    {
        var bookmark = await _context.Bookmarks
            .FirstOrDefaultAsync(b => b.UserId == userId && b.QuestionId == questionId);

        if (bookmark != null)
        {
            _context.Bookmarks.Remove(bookmark);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<BookmarkDto>> GetUserBookmarksAsync(int userId)
    {
        return await _context.Bookmarks
            .Where(b => b.UserId == userId)
            .Include(b => b.Question)
            .Select(b => new BookmarkDto
            {
                Id = b.Id,
                QuestionId = b.QuestionId,
                QuestionTitle = b.Question.Title,
                CreatedAt = b.CreatedAt
            })
            .ToListAsync();
    }


}
