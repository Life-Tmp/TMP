using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TMPApplication.AttachmentTasks;
using TMPApplication.DTOs.AtachmentDtos;

namespace TMPService.Tasks
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AttachmentController: ControllerBase
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
            if (file == null || file.Length == 0)
            {
                return BadRequest("File is empty or not provided.");
            }

            
                //attachmentDto.FileSize = file.Length;
                //attachmentDto.FileType = file.ContentType;
                var attachment = await _attachmentService.UploadAttachmentAsync(file,taskId);
                return Ok(attachment);
            
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> RemoveFileAttachment(int attachmentId)
        {
            var attachmentDeleted = await _attachmentService.RemoveAttachmentAsync(attachmentId);
            return Ok(attachmentDeleted);
        }

        [HttpGet("download")]
        public async Task<IActionResult> DownloadFileAttachment(int attachmentId)
        {
            var (fileBytes, fileType) = await _attachmentService.DownloadAttachmentAsync(attachmentId);
            
            if(fileBytes == null)
            {
                return NotFound();
            }

            return File(fileBytes, fileType, $"attachment"); //just for testing,the frontend will do this
        }
    }
}
