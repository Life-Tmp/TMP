using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TMP.Application.Comments;
using TMP.Application.DTOs.CommentDtos;

namespace TMP.Service.Comment
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CommentDto>>> GetComments([FromQuery] int? taskId)
        {
            var comments = await _commentService.GetCommentsAsync(taskId);
            return Ok(comments);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<CommentDto>> GetComment(int id)
        {
            var comment = await _commentService.GetCommentByIdAsync(id);
            if (comment == null) return NotFound();

            return Ok(comment);
        }

        [Authorize]
        [HttpGet("my-comments")]
        public async Task<ActionResult<IEnumerable<CommentDto>>> GetMyComments()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var comments = await _commentService.GetCommentsByUserIdAsync(userId);
            return Ok(comments);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<CommentDto>> AddComment([FromBody] AddCommentDto newComment)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var comment = await _commentService.AddCommentAsync(newComment, userId);
            return CreatedAtAction(nameof(GetComment), new { id = comment.Id }, comment);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateComment(int id, [FromBody] AddCommentDto updatedComment)
        {
            var result = await _commentService.UpdateCommentAsync(id, updatedComment);
            if (!result) return NotFound();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var result = await _commentService.DeleteCommentAsync(id);
            if (!result) return NotFound();

            return NoContent();
        }
    }
}
