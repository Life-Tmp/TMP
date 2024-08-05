using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TMP.Application.DTOs.CommentDtos;
using TMPApplication.Interfaces.Comments;

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
        /// <summary>
        /// Retrieves a list of all comments.
        /// </summary>
        /// <returns>200 OK with a list of comments.</returns>
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CommentDto>>> GetComments()
        {
            _logger.LogInformation("Fetching all comments");
            var comments = await _commentService.GetCommentsAsync();
            return Ok(comments);
        }

        /// <summary>
        /// Retrieves a comment by its ID.
        /// </summary>
        /// <param name="id">The ID of the comment to retrieve.</param>
        /// <returns>200 OK with the comment details; 404 Not Found if the comment does not exist.</returns>
        [Authorize]
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

        /// <summary>
        /// Retrieves the comments made by the currently logged-in user.
        /// </summary>
        /// <returns>200 OK with a list of comments; 401 Unauthorized if the user is not authenticated.</returns>
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
        /// <summary>
        /// Adds a new comment.
        /// </summary>
        /// <param name="newComment">The details of the comment to add.</param>
        /// <returns>The created comment.</returns>
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
        /// <summary>
        /// Updates an existing comment.
        /// </summary>
        /// <param name="id">The ID of the comment to update.</param>
        /// <param name="updatedComment">The updated comment details.</param>
        /// <returns>No content if successful, otherwise a not found response.</returns>
        [Authorize]
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
        /// <summary>
        /// Deletes a comment by its ID.
        /// </summary>
        /// <param name="id">The ID of the comment to delete.</param>
        /// <returns>204 No Content if the deletion is successful; 404 Not Found if the comment does not exist.</returns>
        [Authorize]
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
