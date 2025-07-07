using StackOverFlowClone.Models.DTOs.Answer;
using StackOverFlowClone.Models.Entities;
using StackOverFlowClone.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using StackOverFlowClone.Data;
using StackOverFlowClone.Models.DTOs.Vote;

namespace StackOverFlowClone.Services.Implementations
{
    public class AnswerService : IAnswerService
    {
        private readonly AppDbContext _context;

        public AnswerService(AppDbContext context)
        {
            _context = context;
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
            return await _context.Answers
                .Where(a => a.QuestionId == questionId)
                .Skip((pageNumber-1)*size)
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
