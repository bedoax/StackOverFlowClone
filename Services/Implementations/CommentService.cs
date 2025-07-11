using Microsoft.EntityFrameworkCore;
using StackOverFlowClone.Data;
using StackOverFlowClone.Models.DTOs.Comment;
using StackOverFlowClone.Models.Entities;
using StackOverFlowClone.Models.Enum;
using StackOverFlowClone.Services.Interfaces;

namespace StackOverFlowClone.Services.Implementations
{
    public class CommentService : ICommentService
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly IMentionService _mentionService;

        public CommentService(AppDbContext context, INotificationService notificationService, IMentionService mentionService)
        {
            _context = context;
            _notificationService = notificationService;
            _mentionService = mentionService;
        }

        public async Task<CommentDto> CreateCommentForQuestionAsync(int questionId, CreateCommentDto commentDto, int userId)
        {
            var question = await _context.Questions.FindAsync(questionId);
            if (question == null)
                throw new ArgumentException($"Question with ID {questionId} not found.");

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new ArgumentException($"User with ID {userId} not found.");
            if (user.IsBanned)
            {
                throw new ArgumentException($"You are Banned , you can make a comment after {user.BannedUntil.Value.Month}/{user.BannedUntil.Value.Day}.");
            }
            var comment = new Comment
            {
                Body = commentDto.Body,
                TargetType = TargetType.Question,
                TargetId = questionId,
                UserId = userId
            };
            
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            // بعد حفظ الكومنت

            // 1. استخراج كل الـ mentions
            var mentionedUserIds = await _mentionService.HandleMentionsAsync(comment.Body);

            // 2. إرسال إشعارات لكل اللي اتعملهم mention
            foreach (var mentionedUserId in mentionedUserIds.Distinct())
            {
                if (mentionedUserId != userId) // ما تبعتش لنفس الشخص
                {
                    var notification = new Notification
                    {
                        UserId = mentionedUserId,
                        Title = "You were mentioned in a comment",
                        Message = $"{user.UserName} mentioned you in a comment.",
                        Type = NotificationType.Mention,
                        TargetId = question.Id,
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false
                    };
                    await _notificationService.SendNotificationAsync(notification);
                }
            }

            // 3. إشعار لصاحب السؤال لو مش هو اللي عمل الكومنت
            if (question.UserId != userId && !mentionedUserIds.Contains(question.UserId))
            {
                var notification = new Notification
                {
                    UserId = question.UserId,
                    Title = "New comment on your question",
                    Message = $"{user.UserName} commented on your question.",
                    Type = NotificationType.Comment,
                    TargetId = question.Id,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };
                await _notificationService.SendNotificationAsync(notification);
            }



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
            if (user.IsBanned)
            {
                throw new ArgumentException($"You are Banned , you can make a comment after {user.BannedUntil.Value.Month}/{user.BannedUntil.Value.Day}.");
            }
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

        public async Task<bool> UpdateCommentAsync(int commentId, UpdateCommentDto commentDto, int userId)
        {

            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null)
                return false;

            if (comment.UserId != userId)
                throw new UnauthorizedAccessException("You can only delete your own comments.");
            var user = await _context.Users.FindAsync(userId);
            if (user.IsBanned)
            {
                throw new ArgumentException($"You are Banned , you can make a comment after {user.BannedUntil.Value.Month}/{user.BannedUntil.Value.Day}.");
            }
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
