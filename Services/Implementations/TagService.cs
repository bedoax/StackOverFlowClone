using Microsoft.EntityFrameworkCore;
using StackOverFlowClone.Data;
using StackOverFlowClone.Models.DTOs.Tag;
using StackOverFlowClone.Models.Entities;
using StackOverFlowClone.Services.Interfaces;

namespace StackOverFlowClone.Services.Implementations
{
    public class TagService : ITagService
    {
        private readonly AppDbContext _context;

        public TagService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<TagDto> CreateTagAsync(CreateTagDto dto)
        {
            var tagEntity = new Tag
            {
                Name = dto.Name,
            };

            _context.Tags.Add(tagEntity);
            await _context.SaveChangesAsync();

            return new TagDto
            {
                Id = tagEntity.Id,
                Name = tagEntity.Name
            };
        }

        public async Task<bool> DeleteTagAsync(int id)
        {
            var tag = await _context.Tags.FindAsync(id);
            if (tag == null)
                return false;

            _context.Tags.Remove(tag);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteTagAsync(string name)
        {
            var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == name);
            if (tag == null)
                return false;

            _context.Tags.Remove(tag);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<TagDto>> GetAllTagsAsync()
        {
            var tags = await _context.Tags.ToListAsync();
            return tags.Select(tag => new TagDto { Id = tag.Id, Name = tag.Name });
        }

        public async Task<int> GetQuestionsCountForTagAsync(string tagName)
        {
            if (string.IsNullOrWhiteSpace(tagName))
                throw new ArgumentException("Tag name cannot be null or empty.", nameof(tagName));

            var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == tagName);
            if (tag == null)
                throw new KeyNotFoundException($"Tag with name '{tagName}' not found.");

            return await _context.QuestionTags.CountAsync(qt => qt.TagId == tag.Id);
        }

        public async Task<TagDto> GetTagByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid tag ID", nameof(id));

            var tag = await _context.Tags.FirstOrDefaultAsync(x => x.Id == id);
            if (tag == null)
                throw new KeyNotFoundException($"Tag with ID {id} not found.");

            return new TagDto { Id = tag.Id, Name = tag.Name };
        }

        public async Task<TagDto> GetTagByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Tag name cannot be null or empty.", nameof(name));

            var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == name);
            if (tag == null)
                throw new KeyNotFoundException($"Tag with name '{name}' not found.");

            return new TagDto { Id = tag.Id, Name = tag.Name };
        }
    }
}
