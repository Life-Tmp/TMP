using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TMP.Application.DTOs.TagDtos;
using TMP.Application.Interfaces.Tags;

namespace TMP.Service.Controllers.Tags
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
        public async Task<ActionResult<IEnumerable<TagDto>>> GetTags()
        {
            var tags = await _tagService.GetAllTagsAsync();
            return Ok(tags);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TagDto>> GetTag(int id)
        {
            var tag = await _tagService.GetTagByIdAsync(id);
            if (tag == null) return NotFound();

            return Ok(tag);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<TagDto>> AddTag(AddTagDto newTag)
        {
            var tag = await _tagService.AddTagAsync(newTag);
            return CreatedAtAction(nameof(GetTag), new { id = tag.Id }, tag);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTag(int id, AddTagDto updatedTag)
        {
            var result = await _tagService.UpdateTagAsync(id, updatedTag);
            if (!result) return NotFound();

            return NoContent();
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTag(int id)
        {
            var result = await _tagService.DeleteTagAsync(id);
            if (!result) return NotFound();

            return NoContent();
        }
    }
}
