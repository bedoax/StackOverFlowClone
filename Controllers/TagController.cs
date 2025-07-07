using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StackOverFlowClone.Models.DTOs.Tag;
using StackOverFlowClone.Services.Interfaces;

namespace StackOverFlowClone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TagController : ControllerBase
    {
        private readonly ITagService _tagService;
            public TagController(ITagService tagService)
            {
                _tagService = tagService;
            }
    
            [HttpGet]
            public async Task<IActionResult> GetAllTags()
            {
                var tags = await _tagService.GetAllTagsAsync();
                return Ok(tags);
            }
    
            [HttpGet("{name}")]
            public async Task<IActionResult> GetTagByName(string name)
            {
                var tag = await _tagService.GetTagByNameAsync(name);
                return tag != null ? Ok(tag) : NotFound();
            }
    
            [HttpPost]
            public async Task<IActionResult> CreateTag([FromBody] CreateTagDto createTagDto)
            {
                var createdTag = await _tagService.CreateTagAsync(createTagDto);
                return CreatedAtAction(nameof(GetTagByName), new { name = createdTag.Name }, createdTag);
            }
    
            [HttpDelete("{name}")]
            public async Task<IActionResult> DeleteTag(string name)
            {
                var success = await _tagService.DeleteTagAsync(name);
                return success ? NoContent() : NotFound();
        }
    }
}
