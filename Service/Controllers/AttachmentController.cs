using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TMPApplication.AttachmentTasks;
using TMPApplication.DTOs.AtachmentDtos;
using TMPDomain.Exceptions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace TMPService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AttachmentController : ControllerBase
    {
        private readonly IAttachmentService _attachmentService;
        private readonly ILogger<AttachmentController> _logger;

        public AttachmentController(IAttachmentService attachmentService, ILogger<AttachmentController> logger)
        {
            _attachmentService = attachmentService;
            _logger = logger;
        }

        #region Create

        /// <summary>
        /// Uploads a file attachment for a specified task.
        /// </summary>
        /// <param name="file">The file to upload.</param>
        /// <param name="taskId">The ID of the task to associate with the attachment.</param>
        /// <returns>An IActionResult indicating the result of the upload operation</returns>
        /// <response code="200">File attachment uploaded successfully</response>
        /// <response code="400">File is empty or not provided</response>
        /// <response code="500">An unexpected error occurred</response> 
        [HttpPost("upload")]
        [Authorize]
        public async Task<IActionResult> UploadFileAttachment(IFormFile file, int taskId)
        {
            _logger.LogInformation("Uploading file attachment for task ID: {TaskId}", taskId);

            try
            {
                if (file == null || file.Length == 0)
                {
                    _logger.LogWarning("File is empty or not provided");
                    return BadRequest("File is empty or not provided.");
                }

                var attachment = await _attachmentService.UploadAttachmentAsync(file, taskId);
                _logger.LogInformation("File attachment uploaded successfully for task ID: {TaskId}", taskId);
                return Ok(attachment);
            }
            catch (AttachmentException ex)
            {
                _logger.LogWarning("AttachmentException: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while uploading file attachment for task ID: {TaskId}", taskId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred");
            }
        }
        #endregion

        #region Read
        /// <summary>
        /// Retrieves all file attachments associated with a specific task.
        /// </summary>
        /// <param name="id">The ID of the task to retrieve attachments for.</param>
        /// <returns>A list of attachments associated with the specified task.</returns>
        /// <response code="200">Returns the list of attachments.</response>
        /// <response code="404">No attachments found for the specified task.</response>
        [HttpGet("task/{id}")]
        [Authorize]
        public async Task<IActionResult> GetAllTaskAttachments(int id)
        {
            var attachments = await _attachmentService.GetAttachmentsAsync(id);
            if (attachments == null)
                return NotFound("There is no attachments for this Task");

            return Ok(attachments);

        }

        /// <summary>
        /// Downloads a file attachment by its ID.
        /// </summary>
        /// <param name="attachmentId">The ID of the attachment to download.</param>
        /// <returns>The file content of the attachment.</returns>
        /// <response code="200">File attachment downloaded successfully.</response>
        /// <response code="404">File attachment not found.</response>
        /// <response code="500">An unexpected error occurred.</response>
        [HttpGet("download")]
        [Authorize]
        public async Task<IActionResult> DownloadFileAttachment(int attachmentId)
        {
            _logger.LogInformation("Downloading file attachment with ID: {AttachmentId}", attachmentId);

            try
            {
                var file = await _attachmentService.DownloadAttachmentAsync(attachmentId);

                if (file == null)
                {
                    _logger.LogWarning("File attachment with ID: {AttachmentId} not found", attachmentId);
                    return NotFound();
                }

                _logger.LogInformation("File attachment with ID: {AttachmentId} downloaded successfully", attachmentId);
                return File(file.FileBytes, file.FileType, file.FileName);
            }
            catch (AttachmentException ex)
            {
                _logger.LogWarning("AttachmentException: {Message}", ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while downloading file attachment with ID: {AttachmentId}", attachmentId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred");
            }
        }
        #endregion

        #region Delete
        /// <summary>
        /// Removes a file attachment by its ID.
        /// </summary>
        /// <param name="attachmentId">The ID of the attachment to remove.</param>
        /// <returns>An IActionResult indicating the result of the delete operation.</returns>
        /// <response code="200">File attachment removed successfully, you did it</response>
        /// <response code="404">File attachment not found</response>
        /// <response code="500">An unexpected error occurred</response>
        [HttpDelete("delete")]
        [Authorize]
        public async Task<IActionResult> RemoveFileAttachment(int attachmentId)
        {
            _logger.LogInformation("Removing file attachment with ID: {AttachmentId}", attachmentId);

            try
            {
                var attachmentDeleted = await _attachmentService.RemoveAttachmentAsync(attachmentId);
                _logger.LogInformation("File attachment with ID: {AttachmentId} removed successfully", attachmentId);
                return Ok(attachmentDeleted);
            }
            catch (AttachmentException ex)
            {
                _logger.LogWarning("AttachmentException: {Message}", ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while removing file attachment with ID: {AttachmentId}", attachmentId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred");
            }
        }
        #endregion
    }
}
