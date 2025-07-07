using StackOverFlowClone.Models.DTOs.Bookmark;

namespace StackOverFlowClone.Services.Interfaces
{
    public interface IBookmarkService
    {
        Task AddBookmarkAsync(int userId, int questionId);
        Task RemoveBookmarkAsync(int userId, int questionId);
        Task<List<BookmarkDto>> GetUserBookmarksAsync(int userId);
    }

}
