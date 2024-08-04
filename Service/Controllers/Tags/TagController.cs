using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TMP.Application.DTOs.TagDtos;
using TMP.Application.Interfaces.Tags;
using TMPApplication.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TMP.Service.Controllers.Tags
{
    [Route("api/[controller]")]
    [ApiController]
    public class TagController : ControllerBase
    {
        private readonly ITagService _tagService;
        private readonly ISearchService<TagDto> _searchService;
        private readonly ILogger<TagController> _logger;

        public TagController(ITagService tagService, ISearchService<TagDto> searchService, ILogger<TagController> logger)
        {
            _tagService = tagService;
            _searchService = searchService;
            _logger = logger;
        }

        #region Read
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TagDto>>> GetTags()
        {
            _logger.LogInformation("Fetching all tags");

            var tags = await _tagService.GetAllTagsAsync();
            return Ok(tags);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TagDto>> GetTag(int id)
        {
            _logger.LogInformation("Fetching tag with ID: {TagId}", id);

            var tag = await _tagService.GetTagByIdAsync(id);
            if (tag == null)
            {
                _logger.LogWarning("Tag with ID: {TagId} not found", id);
                return NotFound();
            }

            return Ok(tag);
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<TagDto>>> SearchTags([FromQuery] string searchTerm)
        {
            _logger.LogInformation("Searching tags with search term: {SearchTerm}", searchTerm);

            var tags = await _searchService.SearchDocumentAsync(searchTerm, "tags");
            return Ok(tags);
        }
        #endregion

        #region Create
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<TagDto>> AddTag(AddTagDto newTag)
        {
            _logger.LogInformation("Adding new tag");

            var tag = await _tagService.AddTagAsync(newTag);
            return CreatedAtAction(nameof(GetTag), new { id = tag.Id }, tag);
        }
        #endregion

        #region Update
        [Authorize]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateTag(int id, AddTagDto updatedTag)
        {
            _logger.LogInformation("Updating tag with ID: {TagId}", id);

            var result = await _tagService.UpdateTagAsync(id, updatedTag);
            if (!result)
            {
                _logger.LogWarning("Tag with ID: {TagId} not found", id);
                return NotFound();
            }

            return NoContent();
        }
        #endregion

        #region Delete
        [Authorize]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteTag(int id)
        {
            _logger.LogInformation("Deleting tag with ID: {TagId}", id);

            var result = await _tagService.DeleteTagAsync(id);
            if (!result)
            {
                _logger.LogWarning("Tag with ID: {TagId} not found", id);
                return NotFound();
            }

            return NoContent();
        }
        #endregion
    }
}
