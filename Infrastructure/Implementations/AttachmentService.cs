using Amazon.S3;
using Amazon.S3.Transfer;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TMP.Application.Interfaces;
using TMPApplication.AttachmentTasks;
using TMPApplication.DTOs.AtachmentDtos;
using TMPDomain.Entities;
using TMPDomain.Exceptions;

namespace TMPInfrastructure.Implementations
{
    public class AttachmentService : IAttachmentService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
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

        public AttachmentService(IAmazonS3 s3Client, IUnitOfWork unitOfWork, IConfiguration configuration, IMapper mapper)
        {
            _s3Client = s3Client;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _mapper = mapper;
        }

        public async Task<AddAttachmentDto> UploadAttachmentAsync(IFormFile file, int taskId)
        {
            if(file.Length == 0 || taskId == 0 || file.Length > 20000000 || !AllowedFileTypes.Contains(file.ContentType))
            {
                throw new AttachmentException("Invalid file or taskId, or file size exceeds the limit.");
            }
            var bucketName = _configuration["AWS:BucketName"];
            var key = $"{Guid.NewGuid()}_{file.FileName}";
            var fileStream = file.OpenReadStream();
            // Upload to S3
            var transferUtility = new TransferUtility(_s3Client); //CHECK: 
            await transferUtility.UploadAsync(fileStream, bucketName, key);

            var attachment = new Attachment
            {
                FileName = file.FileName,
                FilePath = key,
                FileSize = file.Length, // TODO: use kb, mb
                FileType = file.ContentType,
                UploadDate = DateTime.UtcNow,
                TaskId = taskId 
            };

            _unitOfWork.Repository<Attachment>().Create(attachment);
            _unitOfWork.Complete();

            return _mapper.Map<AddAttachmentDto>(attachment);
        }
        public async Task<bool> RemoveAttachmentAsync(int attachmentId)
        {
            var bucketName = _configuration["AWS:BucketName"];
            var attachment = await _unitOfWork.Repository<Attachment>().GetById(x => x.Id == attachmentId).FirstOrDefaultAsync();
            if (attachment == null)
            {
                throw new AttachmentException("Attachment not found.");
            }
            try
            {
                var deleteObjectResponse = await _s3Client.DeleteObjectAsync(bucketName, attachment.FilePath);

                if (deleteObjectResponse.HttpStatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    _unitOfWork.Repository<Attachment>().Delete(attachment);
                    _unitOfWork.Complete();
                }
                else
                {
                    throw new AttachmentException("Failed to delete attachment from S3");
                }
            }
            catch (Exception e)
            {
                throw new AttachmentException("Error while trying to delete atachment", e);
            }

            return true;
        }
        public async Task<List<Attachment>> GetAttachmentsAsync(int taskId)
        {
            var attachments = await _unitOfWork.Repository<Attachment>().GetByCondition(x => x.TaskId == taskId).ToListAsync();

            if (attachments == null)
                throw new AttachmentException("Attachments not found");

            return attachments;
        }

        public async Task<FileAttachmentDto> DownloadAttachmentAsync(int attachmentId)
        {
            var bucketName = _configuration["AWS:BucketName"];
            var attachment = await _unitOfWork.Repository<Attachment>().GetById(x => x.Id == attachmentId).FirstOrDefaultAsync();
            if (attachment == null)
            {
                throw new AttachmentException("Attachment not found");
            }
            using (var response = await _s3Client.GetObjectAsync(bucketName, attachment.FilePath)) //CHECK
            {
                using (var responseStream = response.ResponseStream)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await responseStream.CopyToAsync(memoryStream);
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
    }
}
