using StackOverFlowClone.Models.DTOs.Answer;
using StackOverFlowClone.Models.Entities;
using StackOverFlowClone.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using StackOverFlowClone.Data;
using StackOverFlowClone.Models.DTOs.Vote;
using StackOverFlowClone.Models.Enum;

namespace StackOverFlowClone.Services.Implementations
{
    public class AnswerService : IAnswerService
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;
        public AnswerService(AppDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<AnswerDto> CreateAnswerAsync(int questionId, CreateAnswerDto answerDto, int userId)
        {
            var question = await _context.Questions.FindAsync(questionId);
            if (question == null) return null;

            var answer = new Answer
            {
                Body = answerDto.Body,
                QuestionId = questionId,
                UserId = userId
            };

            _context.Answers.Add(answer);
            await _context.SaveChangesAsync();
            if (question != null && question.UserId != userId)
            {
                await _notificationService.SendNotificationAsync(new Notification
                {
                    UserId = question.UserId,
                    Title = "New Answer on Your Question",
                    Message = "Someone answered your question.",
                    Type = NotificationType.Answer,
                    TargetId = question.Id,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });
            }

            return new AnswerDto
            {
                Id = answer.Id,
                Body = answer.Body,
                UserId = answer.UserId,
                QuestionId = answer.QuestionId
            };
        }

        public async Task<bool> UpdateAnswerAsync(int answerId, UpdateAnswerDto answerDto)
        {
            var answer = await _context.Answers.FindAsync(answerId);
            if (answer == null) return false;

            answer.Body = answerDto.Body;

            _context.Answers.Update(answer);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAnswerAsync(int answerId, int userId)
        {
            var answer = await _context.Answers.FindAsync(answerId);
            if (answer == null || answer.UserId != userId) return false;
            var comments = await _context.Comments
                .Where(c => c.TargetType == TargetType.Answer && c.TargetId == answerId)
                .ToListAsync();
            var votes = await _context.Votes.Where(v => v.AnswerId == answerId && v.TargetType == TargetType.Answer).ToListAsync();
            // Remove associated comments and votes
            _context.Comments.RemoveRange(comments);
            _context.Votes.RemoveRange(votes);
            // Remove the answer itself
            _context.Answers.Remove(answer);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> VoteAnswerAsync(int answerId, int userId, bool isUpvote)
        {
            var existingVote = await _context.Votes
                .FirstOrDefaultAsync(v => v.AnswerId == answerId && v.UserId == userId && v.TargetType == TargetType.Answer);
            var voteValue = isUpvote ? VoteType.UpVote : VoteType.DownVote;
            if (existingVote != null)
            {
                existingVote.VoteType = voteValue;
                _context.Votes.Update(existingVote);
            }
            else
            {
                var vote = new Vote
                {
                    AnswerId = answerId,
                    UserId = userId,
                    VoteType = voteValue,
                    TargetType = TargetType.Answer,
                    Question = null, // Assuming these are navigation properties, set them to null
                    Answer = null // Assuming these are navigation properties, set them to null
                };
                _context.Votes.Add(vote);
                await _context.SaveChangesAsync();
            }

            
            return true;
        }

        public async Task<IEnumerable<AnswerDto>> GetAnswerForQuestionAsync(int questionId,int pageNumber, int size)
        {
            if (pageNumber <= 0 || size <= 0)
            {
                throw new ArgumentException("Page number and size must be greater than zero.");
            }
          return  await _context.Answers
               .Where(a => a.QuestionId == questionId)
               .Skip((pageNumber - 1) * size)
               .Take(size)
               .Select(a => new AnswerDto
               {
                   Id = a.Id,
                   Body = a.Body,
                   UserId = a.UserId,
                   QuestionId = a.QuestionId
               })
               .ToListAsync();

        }

        public async Task<int> GetAnswerVoteCountAsync(int answerId)
        {
            var upvotes = await _context.Votes.CountAsync(v => v.AnswerId == answerId && v.TargetType == TargetType.Answer && v.VoteType == VoteType.UpVote);
            var downvotes = await _context.Votes.CountAsync(v => v.AnswerId == answerId && v.TargetType == TargetType.Answer && v.VoteType == VoteType.DownVote);
            return upvotes - downvotes;
        }
    }
}
