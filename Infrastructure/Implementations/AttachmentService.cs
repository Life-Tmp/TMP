using FluentValidation;
using Amazon.S3;
using Amazon.S3.Transfer;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TMP.Application.Interfaces;
using TMPDomain.Entities;
using TMPDomain.Exceptions;
using TMPApplication.DTOs.AtachmentDtos;
using TMPApplication.AttachmentTasks;

namespace TMP.Infrastructure.Implementations
{
    public class AttachmentService : IAttachmentService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly ILogger<AttachmentService> _logger;
        private readonly IValidator<Attachment> _attachmentValidator;

        private static readonly HashSet<string> AllowedFileTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/gif",
            "application/pdf",
            "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "application/vnd.ms-powerpoint",
            "application/vnd.openxmlformats-officedocument.presentationml.presentation"
        };

        public AttachmentService(
            IAmazonS3 s3Client,
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            IMapper mapper,
            ILogger<AttachmentService> logger,
            IValidator<Attachment> attachmentValidator)
        {
            _s3Client = s3Client;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _mapper = mapper;
            _logger = logger;
            _attachmentValidator = attachmentValidator;
        }

        #region Upload Methods
        public async Task<AddAttachmentDto> UploadAttachmentAsync(IFormFile file, int taskId)
        {
            _logger.LogInformation("Uploading attachment for task ID: {TaskId}", taskId);

            if (file.Length == 0 || taskId == 0 || file.Length > 20000000 || !AllowedFileTypes.Contains(file.ContentType))
            {
                _logger.LogWarning("Invalid file or taskId, or file size exceeds the limit.");
                throw new AttachmentException("Invalid file or taskId, or file size exceeds the limit.");
            }

            var bucketName = _configuration["AWS:BucketName"];
            var key = $"{Guid.NewGuid()}_{file.FileName}";
            var fileStream = file.OpenReadStream();

            try
            {
                var transferUtility = new TransferUtility(_s3Client);
                await transferUtility.UploadAsync(fileStream, bucketName, key);
                _logger.LogInformation("File uploaded to S3 with key: {FileKey}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file to S3");
                throw new AttachmentException("Error uploading file to S3", ex);
            }

            var attachment = new Attachment
            {
                FileName = file.FileName,
                FilePath = key,
                FileSize = file.Length,
                FileType = file.ContentType,
                UploadDate = DateTime.UtcNow,
                TaskId = taskId
            };

            var validationResult = await _attachmentValidator.ValidateAsync(attachment);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Validation failed for attachment: {Errors}", string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
                throw new ValidationException(validationResult.Errors);
            }

            _unitOfWork.Repository<Attachment>().Create(attachment);
            _unitOfWork.Complete();

            _logger.LogInformation("Attachment record created for task ID: {TaskId}", taskId);

            return _mapper.Map<AddAttachmentDto>(attachment);
        }
        #endregion

        #region Delete Methods
        public async Task<bool> RemoveAttachmentAsync(int attachmentId)
        {
            _logger.LogInformation("Removing attachment with ID: {AttachmentId}", attachmentId);

            var bucketName = _configuration["AWS:BucketName"];
            var attachment = await _unitOfWork.Repository<Attachment>().GetById(x => x.Id == attachmentId).FirstOrDefaultAsync();
            if (attachment == null)
            {
                _logger.LogWarning("Attachment with ID: {AttachmentId} not found", attachmentId);
                throw new AttachmentException("Attachment not found.");
            }

            try
            {
                var deleteObjectResponse = await _s3Client.DeleteObjectAsync(bucketName, attachment.FilePath);
                if (deleteObjectResponse.HttpStatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    _unitOfWork.Repository<Attachment>().Delete(attachment);
                    _unitOfWork.Complete();
                    _logger.LogInformation("Attachment with ID: {AttachmentId} deleted successfully", attachmentId);
                }
                else
                {
                    _logger.LogWarning("Failed to delete attachment with ID: {AttachmentId} from S3", attachmentId);
                    throw new AttachmentException("Failed to delete attachment from S3");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting attachment with ID: {AttachmentId} from S3", attachmentId);
                throw new AttachmentException("Error while trying to delete attachment", ex);
            }

            return true;
        }
        #endregion

        #region Get Methods
        public async Task<List<Attachment>> GetAttachmentsAsync(int taskId)
        {
            _logger.LogInformation("Fetching attachments for task ID: {TaskId}", taskId);

            var attachments = await _unitOfWork.Repository<Attachment>().GetByCondition(x => x.TaskId == taskId).ToListAsync();
            if (attachments == null || !attachments.Any())
            {
                _logger.LogWarning("No attachments found for task ID: {TaskId}", taskId);
                throw new AttachmentException("Attachments not found");
            }

            _logger.LogInformation("Fetched {Count} attachments for task ID: {TaskId}", attachments.Count, taskId);
            return attachments;
        }

        public async Task<FileAttachmentDto> DownloadAttachmentAsync(int attachmentId)
        {
            _logger.LogInformation("Downloading attachment with ID: {AttachmentId}", attachmentId);

            var bucketName = _configuration["AWS:BucketName"];
            var attachment = await _unitOfWork.Repository<Attachment>().GetById(x => x.Id == attachmentId).FirstOrDefaultAsync();
            if (attachment == null)
            {
                _logger.LogWarning("Attachment with ID: {AttachmentId} not found", attachmentId);
                throw new AttachmentException("Attachment not found");
            }

            try
            {
                using (var response = await _s3Client.GetObjectAsync(bucketName, attachment.FilePath))
                {
                    using (var responseStream = response.ResponseStream)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await responseStream.CopyToAsync(memoryStream);
                            _logger.LogInformation("Attachment with ID: {AttachmentId} downloaded successfully", attachmentId);
                            return new FileAttachmentDto
                            {
                                FileBytes = memoryStream.ToArray(),
                                FileType = attachment.FileType,
                                FileName = attachment.FileName
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading attachment with ID: {AttachmentId}", attachmentId);
                throw new AttachmentException("Error downloading attachment", ex);
            }
        }
        #endregion
    }
}
