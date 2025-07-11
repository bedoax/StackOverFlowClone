using StackOverFlowClone.Models.DTOs.Comment;

namespace StackOverFlowClone.Services.Interfaces
{
    public interface ICommentService
    {
        Task<CommentDto> CreateCommentForQuestionAsync(int questionId, CreateCommentDto commentDto, int userId);
        Task<CommentDto> CreateCommentForAnswerAsync(int answerId, CreateCommentDto commentDto, int userId);
        Task<bool> UpdateCommentAsync(int commentId, UpdateCommentDto commentDto, int userId);
        Task<bool> DeleteCommentAsync(int commentId, int userId);
        Task<IEnumerable<CommentDto>> GetCommentForQuestionAsync(int questionId);
        Task<IEnumerable<CommentDto>> GetCommentForAnswerAsync(int answerId);
    }
}
