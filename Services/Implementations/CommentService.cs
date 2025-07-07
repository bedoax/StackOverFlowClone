using Microsoft.EntityFrameworkCore;
using StackOverFlowClone.Data;
using StackOverFlowClone.Models.DTOs.Comment;
using StackOverFlowClone.Models.Entities;
using StackOverFlowClone.Services.Interfaces;

namespace StackOverFlowClone.Services.Implementations
{
    public class CommentService : ICommentService
    {
        private readonly AppDbContext _context;

        public CommentService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CommentDto> CreateCommentForQuestionAsync(int questionId, CreateCommentDto commentDto, int userId)
        {
            var question = await _context.Questions.FindAsync(questionId);
            if (question == null)
                throw new ArgumentException($"Question with ID {questionId} not found.");

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new ArgumentException($"User with ID {userId} not found.");

            var comment = new Comment
            {
                Body = commentDto.Body,
                TargetType = TargetType.Question,
                TargetId = questionId,
                UserId = userId
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            var createdComment = await _context.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == comment.Id);

            return MapToCommentDto(createdComment);
        }

        public async Task<CommentDto> CreateCommentForAnswerAsync(int answerId, CreateCommentDto commentDto, int userId)
        {
            var answer = await _context.Answers.FindAsync(answerId);
            if (answer == null)
                throw new ArgumentException($"Answer with ID {answerId} not found.");

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new ArgumentException($"User with ID {userId} not found.");

            var comment = new Comment
            {
                Body = commentDto.Body,
                TargetType = TargetType.Answer,
                TargetId = answerId,
                UserId = userId
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            var createdComment = await _context.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == comment.Id);

            return MapToCommentDto(createdComment);
        }

        public async Task<bool> UpdateCommentAsync(int commentId, UpdateCommentDto commentDto)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null)
                return false;

            comment.Body = commentDto.Body;

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteCommentAsync(int commentId, int userId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null)
                return false;

            if (comment.UserId != userId)
                throw new UnauthorizedAccessException("You can only delete your own comments.");

            _context.Comments.Remove(comment);

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<CommentDto>> GetCommentForQuestionAsync(int questionId)
        {
            var comments = await _context.Comments
                .Where(c => c.TargetType == TargetType.Question && c.TargetId == questionId)
                .ToListAsync();

            return comments.Select(MapToCommentDto);
        }

        public async Task<IEnumerable<CommentDto>> GetCommentForAnswerAsync(int answerId)
        {
            var comments = await _context.Comments
                .Where(c => c.TargetType == TargetType.Answer && c.TargetId == answerId)
                .ToListAsync();

            return comments.Select(MapToCommentDto);
        }

        private CommentDto MapToCommentDto(Comment comment)
        {
            return new CommentDto
            {
                Id = comment.Id,
                Body = comment.Body,
                UserId = comment.UserId
            };
        }
    }

}
