using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using StackOverFlowClone.Models.DTOs.Bookmark;
using StackOverFlowClone.Services.Interfaces;
using System.Security.Claims;

[ApiController]
[Route("api/bookmarks")]
public class BookmarkController : ControllerBase
{
    private readonly IBookmarkService _bookmarkService;


    public BookmarkController(IBookmarkService bookmarkService)
    {
        _bookmarkService = bookmarkService;

        
    }
    [Authorize(Roles = "user")]
    [HttpPost]
    public async Task<IActionResult> AddBookmark([FromBody] BookmarkCreateDto dto)
    {
        var userId = GetUserIdFromClaims();
        await _bookmarkService.AddBookmarkAsync(userId, dto.QuestionId);
        return Ok(new { message = "Bookmark added." });
    }
    [Authorize(Roles = "user")]
    [HttpDelete("{questionId}")]
    public async Task<IActionResult> RemoveBookmark(int questionId)
    {
        var userId = GetUserIdFromClaims();
        await _bookmarkService.RemoveBookmarkAsync(userId, questionId);
        return Ok(new { message = "Bookmark removed." });
    }
    [Authorize(Roles = "user")]
    [HttpGet]
    public async Task<IActionResult> GetUserBookmarks()
    {
        //var userId1 = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var userId = GetUserIdFromClaims();
        var bookmarks = await _bookmarkService.GetUserBookmarksAsync(userId);
        return Ok(bookmarks);
    }
    private int GetUserIdFromClaims()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }
        return int.Parse(userIdClaim.Value);
    }
}




