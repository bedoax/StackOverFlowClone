using Microsoft.EntityFrameworkCore;
using StackOverFlowClone.Data;
using StackOverFlowClone.Models.DTOs.Vote;
using StackOverFlowClone.Models.Enum;
using StackOverFlowClone.Services.Interfaces;

namespace StackOverFlowClone.Services.Implementations
{
    public class VoteService : IVoteService
    {
        private readonly AppDbContext _context;

        public VoteService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> VoteAsync(int targetId, TargetType targetType, int userId, bool isUpvote)
        {
            var existingVote = await _context.Votes.AsNoTracking()
                .FirstOrDefaultAsync(v => (v.QuestionId == targetId || v.AnswerId == targetId) && v.TargetType == targetType && v.UserId == userId);

            var newVoteType = isUpvote ? VoteType.UpVote : VoteType.DownVote;

            if (existingVote != null)
            {
                // If user is voting the same way again, don't change anything
                if (existingVote.VoteType == newVoteType)
                    return false;

                // Update existing vote
                existingVote.VoteType = newVoteType;
                _context.Votes.Update(existingVote);
            }
            else
            {
                // Create new vote
                var newVote = new Vote
                {
                    QuestionId = targetId,
                    TargetType = targetType,
                    VoteType = newVoteType,
                    UserId = userId,
                    Question = null ,
                    Answer = null // Assuming these are navigation properties, set them to null

                };
                await _context.Votes.AddAsync(newVote);
                await _context.SaveChangesAsync();
            }
            
            return true;
        }

        public async Task<bool> UndoVoteAsync(int targetId, TargetType targetType, int userId)
        {
            var vote = await _context.Votes
                .FirstOrDefaultAsync(v => (v.QuestionId == targetId || v.AnswerId == targetId) && v.TargetType == targetType && v.UserId == userId);

            if (vote == null)
                return false;

            _context.Votes.Remove(vote);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<int> GetVoteCountAsync(int targetId, TargetType targetType)
        {
            var upvotes = await _context.Votes
                .CountAsync(v => (v.QuestionId == targetId || v.AnswerId == targetId) && v.TargetType == targetType && v.VoteType == VoteType.UpVote);

            var downvotes = await _context.Votes
                .CountAsync(v => (v.QuestionId == targetId || v.AnswerId == targetId) && v.TargetType == targetType && v.VoteType == VoteType.DownVote);

            return upvotes - downvotes;
        }

        public async Task<int?> GetUserVoteAsync(int targetId, TargetType targetType, int userId)
        {
            var vote = await _context.Votes
                .FirstOrDefaultAsync(v => (v.QuestionId == targetId || v.AnswerId == targetId) && v.TargetType == targetType && v.UserId == userId);

            return vote != null ? (int)vote.VoteType : null;
        }
    }
}




