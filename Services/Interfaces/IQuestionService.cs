using Microsoft.EntityFrameworkCore;
using StackOverFlowClone.Models.DTOs.Question;
using StackOverFlowClone.Models.DTOs.Tag;
using StackOverFlowClone.Models.Entities;

namespace StackOverFlowClone.Services.Interfaces
{
    public interface IQuestionService
    {
        Task<QuestionDto> CreateQuestionAsync(CreateQuestionDto questionDto, int userId);
        Task<QuestionDto> GetQuestionByIdAsync(int questionId);
        Task<IEnumerable<QuestionDto>> GetAllQuestionsAsync(int pageNumber, int size);
        Task<IEnumerable<QuestionDto>> GetQuestionsByNewsetDate(int pageNumber, int size);
        Task<IEnumerable<QuestionDto>> GetQuestionsByMostVoted(int pageNumber, int size);
        Task<IEnumerable<QuestionDto>> GetQuestionsByPapular(int pageNumber, int size);
        Task<IEnumerable<QuestionDto>> GetQuestionsByTag(string tag, int pageNumber, int size);

        Task<IEnumerable<QuestionDto>> GetQuestionsByDateRange(DateTime start, DateTime end, int pageNumber, int size);

        Task<IEnumerable<QuestionDto>> GetQuestionsWithVotesMoreThan(int numberOfVotes, int pageNumber, int size);

        Task<bool> UpdateQuestionAsync(int questionId, UpdateQuestionDto questionDto,int userId);
        Task<bool> DeleteQuestionAsync(int questionId, int userId);
        Task<bool> VoteQuestionAsync(int questionId, int userId, bool isUpvote);
        Task<int> GetQuestionVoteCountAsync(int questionId);
        Task<bool> DeleteAllQuestions();
        //Task<IEnumerable<QuestionDto>> GetQuestionsByTagAsync(string tagName);
        Task<int> GetQuestionsCountForTagAsync(string tag);
        Task<List<QuestionDto>> GetQuestionsWithTagAsync(string tag);
    }

}
