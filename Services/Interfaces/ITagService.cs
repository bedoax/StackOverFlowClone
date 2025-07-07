using StackOverFlowClone.Models.DTOs.Question;
using StackOverFlowClone.Models.DTOs.Tag;

namespace StackOverFlowClone.Services.Interfaces
{
    public interface ITagService
    {
        Task<IEnumerable<TagDto>> GetAllTagsAsync();
        Task<TagDto> GetTagByIdAsync(int id);
        Task<TagDto> GetTagByNameAsync(string name);
        Task<TagDto> CreateTagAsync(CreateTagDto dto);
        Task<bool> DeleteTagAsync(int id);
        Task<bool> DeleteTagAsync(string name);
        Task<int> GetQuestionsCountForTagAsync(string tagName);
    }

}
