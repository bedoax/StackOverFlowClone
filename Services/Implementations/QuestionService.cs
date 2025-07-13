using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using StackOverFlowClone.Data;
using StackOverFlowClone.Models.DTOs.Question;
using StackOverFlowClone.Models.Entities;
using StackOverFlowClone.Models.Enum;
using StackOverFlowClone.Services.Interfaces;
using System.Linq;

namespace StackOverFlowClone.Services.Implementations
{
    public class QuestionService : IQuestionService
    {
        private readonly AppDbContext _context;
        private readonly IVoteService _voteService;
        private readonly IMemoryCache _cache;
        public QuestionService(AppDbContext context, IVoteService voteService,IMemoryCache cache)
        {
            _context = context;
            _voteService = voteService;
            _cache = cache;
        }

        public async Task<QuestionDto> CreateQuestionAsync(CreateQuestionDto questionDto, int userId)
        {
            var question = new Question
            {
                Title = questionDto.Title,
                Body = questionDto.Body,
                UserId = userId,
                QuestionTags = questionDto.Tags != null && questionDto.Tags.Any() ?
                    questionDto.Tags.Select(tag => new QuestionTag
                    {
                        Tag = new Tag { Name = tag }
                    }).ToList() : new List<QuestionTag>()
            };

            await _context.Questions.AddAsync(question);
            await _context.SaveChangesAsync();

            return await MapToQuestionDto(question);
        }

        private async Task<IEnumerable<QuestionDto>> GetQuestionsBaseQuery(IQueryable<Question> baseQuery)
        {
            var questions = await baseQuery
                .AsNoTracking()
                .Include(q => q.User)
                .Include(q => q.Answers)
                .Include(q => q.QuestionTags).ThenInclude(qt => qt.Tag)
                .ToListAsync();

            var questionIds = questions.Select(q => q.Id).ToList();

            var voteCounts = await _context.Votes
                .Where(v => questionIds.Contains((int)v.QuestionId) && v.TargetType == TargetType.Question)
                .GroupBy(v => v.QuestionId)
                .Select(g => new { QuestionId = g.Key, Count = g.Sum(v => v.VoteType == VoteType.UpVote ? 1 : -1) })
                .ToDictionaryAsync(g => g.QuestionId, g => g.Count);

            return questions.Select(q => new QuestionDto
            {
                Id = q.Id,
                Title = q.Title,
                Body = q.Body,
                UserId = q.UserId,
                UserName = q.User?.UserName ?? "Unknown",
                AnswerCount = q.Answers?.Count ?? 0,
                VoteCount = voteCounts.ContainsKey(q.Id) ? voteCounts[q.Id] : 0,
                Tags = q.QuestionTags.Select(qt => qt.Tag.Name).ToList()
            });
        }

        public async Task<IEnumerable<QuestionDto>> GetAllQuestionsAsync(int pageNumber, int size)
        {
            var query = _context.Questions
                .OrderByDescending(q => q.Id)
                .Skip((pageNumber - 1) * size)
                .Take(size);

            return await GetQuestionsBaseQuery(query);
        }

        public async Task<IEnumerable<QuestionDto>> GetQuestionsByNewsetDate(int pageNumber, int size)
        {
            var query = _context.Questions
                .OrderByDescending(q => q.CreatedAt)
                .Skip((pageNumber - 1) * size)
                .Take(size);

            return await GetQuestionsBaseQuery(query);
        }

        public async Task<IEnumerable<QuestionDto>> GetQuestionsByPapular(int pageNumber, int size)
        {
            string cacheKey = $"popular_questions_page_{pageNumber}_size_{size}";

            if (pageNumber == 1 && size == 10 && _cache.TryGetValue(cacheKey, out IEnumerable<QuestionDto> cachedQuestions))
            {
                return cachedQuestions;
            }
            var query = _context.Questions
                .OrderByDescending(q => _context.Votes.Count(v => v.QuestionId == q.Id && v.TargetType == TargetType.Question && v.VoteType == VoteType.UpVote))
                .Skip((pageNumber - 1) * size)
                .Take(size);
            var result = await GetQuestionsBaseQuery(query);

            
            if (pageNumber == 1 && size == 10)
            {
                var options = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));

                _cache.Set(cacheKey, result, options);
            }

            return result;
        }

        public async Task<IEnumerable<QuestionDto>> GetQuestionsByMostVoted(int pageNumber, int size)
        {
            var query = _context.Questions
                .OrderByDescending(q => _context.Votes.Count(v => v.QuestionId == q.Id && v.TargetType == TargetType.Question))
                .Skip((pageNumber - 1) * size)
                .Take(size);

            return await GetQuestionsBaseQuery(query);
        }

        public async Task<QuestionDto> GetQuestionByIdAsync(int questionId)
        {
            var question = await _context.Questions
                .Include(q => q.User)
                .Include(q => q.Answers)
                .Include(q => q.QuestionTags).ThenInclude(qt => qt.Tag)
                .FirstOrDefaultAsync(q => q.Id == questionId);

            if (question == null)
                return null;

            return await MapToQuestionDto(question);
        }

        public async Task<IEnumerable<QuestionDto>> GetQuestionsByTag(string tag, int pageNumber, int size)
        {
            var tagEntity = await _context.Tags
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Name == tag);

            if (tagEntity == null)
                return Enumerable.Empty<QuestionDto>();

            var query = _context.Questions
                .Where(q => q.QuestionTags.Any(qt => qt.TagId == tagEntity.Id))
                .OrderByDescending(q => q.Id)
                .Skip((pageNumber - 1) * size)
                .Take(size);

            return await GetQuestionsBaseQuery(query);
        }

        public async Task<IEnumerable<QuestionDto>> GetQuestionsByDateRange(DateTime start, DateTime end, int pageNumber, int size)
        {
            if (start > end)
                throw new ArgumentException("Start date must be earlier than or equal to end date.");

            var query = _context.Questions
                .Where(q => q.CreatedAt >= start && q.CreatedAt <= end)
                .OrderByDescending(q => q.CreatedAt)
                .Skip((pageNumber - 1) * size)
                .Take(size);

            return await GetQuestionsBaseQuery(query);
        }

        public async Task<IEnumerable<QuestionDto>> GetQuestionsWithVotesMoreThan(int numberOfVotes, int pageNumber, int size)
        {
            var questionIds = await _context.Votes
                .AsNoTracking()
                .Where(v => v.TargetType == TargetType.Question)
                .GroupBy(v => v.QuestionId)
                .Where(g => g.Count() > numberOfVotes)
                .OrderByDescending(g => g.Count())
                .Skip((pageNumber - 1) * size)
                .Take(size)
                .Select(g => g.Key)
                .ToListAsync();

            var query = _context.Questions
                .Where(q => questionIds.Contains(q.Id));

            return await GetQuestionsBaseQuery(query);
        }

        public async Task<bool> UpdateQuestionAsync(int questionId, UpdateQuestionDto questionDto, int userId)
        {

            var question = await _context.Questions.FindAsync(questionId);
            if (question == null)
                return false;
            if(question.UserId != userId)
            {
                return false;
            }
            question.Title = questionDto.Title ?? question.Title;
            question.Body = questionDto.Body ?? question.Body;

            _context.Questions.Update(question);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteQuestionAsync(int questionId, int userId)
        {
            var question = await _context.Questions.FindAsync(questionId);
            if (question == null || question.UserId != userId)
                return false;

            _context.Questions.Remove(question);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> VoteQuestionAsync(int questionId, int userId, bool isUpvote)
        {
            var questionExists = await _context.Questions.AnyAsync(q => q.Id == questionId);
            if (!questionExists)
                return false;

            return await _voteService.VoteAsync(questionId, TargetType.Question, userId, isUpvote);
        }

        public async Task<bool> DeleteAllQuestions()
        {
            var comments = _context.Comments.ToList();
            var answers = _context.Answers.ToList();
            var questions = _context.Questions.ToList();
            var votes = _context.Votes.ToList();
            _context.Comments.RemoveRange(comments);
            _context.Votes.RemoveRange(votes);
            _context.Answers.RemoveRange(answers);
            _context.Questions.RemoveRange(questions);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<int> GetQuestionVoteCountAsync(int questionId)
        {
            return await _voteService.GetVoteCountAsync(questionId, TargetType.Question);
        }

        private async Task<QuestionDto> MapToQuestionDto(Question question)
        {
            var voteCount = await _voteService.GetVoteCountAsync(question.Id, TargetType.Question);

            var tags = await _context.QuestionTags
                .AsNoTracking()
                .Where(qt => qt.QuestionId == question.Id)
                .Include(qt => qt.Tag)
                .Select(qt => qt.Tag.Name)
                .ToListAsync();

            return new QuestionDto
            {
                Id = question.Id,
                Title = question.Title,
                Body = question.Body,
                UserId = question.UserId,
                UserName = question.User?.UserName ?? "Unknown",
                VoteCount = voteCount,
                AnswerCount = question.Answers?.Count ?? 0,
                Tags = tags
            };
        }

        public async Task<List<QuestionDto>> GetQuestionsWithTagAsync(string tag)
        {
            return await _context.Questions
                .Where(q => q.QuestionTags.Any(qt => qt.Tag.Name == tag))
                .Include(q => q.QuestionTags).ThenInclude(qt => qt.Tag)
                .Select(q => new QuestionDto
                {
                    Id = q.Id,
                    Title = q.Title,
                    Body = q.Body,
                    CreatedAt = q.CreatedAt,
                    Tags = q.QuestionTags.Select(qt => qt.Tag.Name).ToList()
                })
                .ToListAsync();
        }

        public async Task<int> GetQuestionsCountForTagAsync(string tagName)
        {
            if (string.IsNullOrWhiteSpace(tagName))
                return 0;

            var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == tagName);
            if (tag == null)
                return 0;

            return await _context.QuestionTags.CountAsync(qt => qt.TagId == tag.Id);
        }
    }
}
