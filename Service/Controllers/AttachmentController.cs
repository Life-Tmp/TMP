using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TMPApplication.AttachmentTasks;
using TMPApplication.DTOs.AtachmentDtos;
using TMPDomain.Exceptions;

namespace TMPService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AttachmentController : ControllerBase
    {
        private readonly IAttachmentService _attachmentService;

        public AttachmentController(IAttachmentService attachmentService)
        {
            _attachmentService = attachmentService;
        }

        [HttpPost("upload")]
        //[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AddAttachmentDto))]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadFileAttachment(IFormFile file, int taskId)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("File is empty or not provided.");
                }

                var attachment = await _attachmentService.UploadAttachmentAsync(file, taskId);
                return Ok(attachment);
            }
            catch (AttachmentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error ocurred");
            }

        }

        [HttpDelete("delete")]
        public async Task<IActionResult> RemoveFileAttachment(int attachmentId)
        {
            try
            {
                var attachmentDeleted = await _attachmentService.RemoveAttachmentAsync(attachmentId);
                return Ok(attachmentDeleted);
            }
            catch (AttachmentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error");
            }

        }

        [HttpGet("download")]
        public async Task<IActionResult> DownloadFileAttachment(int attachmentId)
        {
            var file = await _attachmentService.DownloadAttachmentAsync(attachmentId);

            if (file == null)
            {
                return NotFound();
            }

            return File(file.FileBytes, file.FileType, file.FileName);
        }
    }
}
