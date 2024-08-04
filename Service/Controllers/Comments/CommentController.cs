using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TMP.Application.Comments;
using TMP.Application.DTOs.CommentDtos;

namespace TMPService.Controllers.Comments
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;
        private readonly ILogger<CommentController> _logger;

        public CommentController(ICommentService commentService, ILogger<CommentController> logger)
        {
            _commentService = commentService;
            _logger = logger;
        }
        
        #region Read
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CommentDto>>> GetComments()
        {
            _logger.LogInformation("Fetching all comments");
            var comments = await _commentService.GetCommentsAsync();
            return Ok(comments);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<CommentDto>> GetComment(int id)
        {
            _logger.LogInformation("Fetching comment with ID: {CommentId}", id);
            var comment = await _commentService.GetCommentByIdAsync(id);
            if (comment == null)
            {
                _logger.LogWarning("Comment with ID: {CommentId} not found", id);
                return NotFound();
            }

            return Ok(comment);
        }

        [Authorize]
        [HttpGet("my-comments")]
        public async Task<ActionResult<IEnumerable<CommentDto>>> GetMyComments()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                _logger.LogWarning("Unauthorized access to my comments endpoint");
                return Unauthorized();
            }

            _logger.LogInformation("Fetching comments for user ID: {UserId}", userId);
            var comments = await _commentService.GetCommentsByUserIdAsync(userId);
            return Ok(comments);
        }
        #endregion

        #region Create
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<CommentDto>> AddComment([FromBody] AddCommentDto newComment)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                _logger.LogWarning("Unauthorized attempt to add a comment");
                return Unauthorized();
            }

            _logger.LogInformation("Adding new comment for user ID: {UserId}", userId);
            var comment = await _commentService.AddCommentAsync(newComment, userId);
            _logger.LogInformation("Comment with ID: {CommentId} added successfully", comment.Id);

            return CreatedAtAction(nameof(GetComment), new { id = comment.Id }, comment);
        }
        #endregion

        #region Update

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateComment(int id, [FromBody] AddCommentDto updatedComment)
        {
            _logger.LogInformation("Updating comment with ID: {CommentId}", id);
            var result = await _commentService.UpdateCommentAsync(id, updatedComment);
            if (!result)
            {
                _logger.LogWarning("Comment with ID: {CommentId} not found for update", id);
                return NotFound();
            }

            _logger.LogInformation("Comment with ID: {CommentId} updated successfully", id);
            return NoContent();
        }
        #endregion

        #region Delete
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            _logger.LogInformation("Deleting comment with ID: {CommentId}", id);
            var result = await _commentService.DeleteCommentAsync(id);
            if (!result)
            {
                _logger.LogWarning("Comment with ID: {CommentId} not found for deletion", id);
                return NotFound();
            }

            _logger.LogInformation("Comment with ID: {CommentId} deleted successfully", id);
            return NoContent();
        }
        #endregion
    }
}
