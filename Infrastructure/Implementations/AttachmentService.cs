using Amazon.Runtime.Internal.Auth;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TMP.Application.Interfaces;
using TMPApplication.AttachmentTasks;
using TMPApplication.DTOs.AtachmentDtos;
using TMPDomain.Entities;

namespace TMPInfrastructure.Implementations
{
    public class AttachmentService : IAttachmentService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public AttachmentService(IAmazonS3 s3Client, IUnitOfWork unitOfWork, IConfiguration configuration, IMapper mapper)
        {
            _s3Client = s3Client;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _mapper = mapper;
        }
        public async Task<AddAttachmentDto> UploadAttachmentAsync(IFormFile file, int taskId)
        {
            if(file.Length == 0 || taskId == 0) //UPDATE: 
            {
                return null;
            }
            var bucketName = _configuration["AWS:BucketName"];  //TODO: DI for bucketName
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
                return false; //TODO: use a response class or something
            }
            try

            {
                
                var deleteObjectResponse = await _s3Client.DeleteObjectAsync(bucketName, attachment.FilePath);

                if (deleteObjectResponse != null) //CHECK: what are directory buckets
                {
                    _unitOfWork.Repository<Attachment>().Delete(attachment);
                    _unitOfWork.Complete();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return true;
        }
        public async Task<List<Attachment>> GetAttachmentsAsync(int taskId)
        {
            var attachments = await _unitOfWork.Repository<Attachment>().GetByCondition(x => x.TaskId == taskId).ToListAsync();

            if (attachments == null)
                return null;

            return attachments; //TODO: Return Dto
        }
        public async Task<(byte[], string)> DownloadAttachmentAsync(int attachmentId)
        {
            var bucketName = _configuration["AWS:BucketName"];
            var attachment = await _unitOfWork.Repository<Attachment>().GetById(x => x.Id == attachmentId).FirstOrDefaultAsync();
            if (attachment == null)
            {
                return (null,""); //TODO: use a response class or something
            }
            //var attachmentType = attachment.FileType.Split("/");
            using (var response = await _s3Client.GetObjectAsync(bucketName, attachment.FilePath)) //CHECK
            {
                using (var responseStream = response.ResponseStream) {
                    using (var memoryStream = new MemoryStream())
                    {
                        await responseStream.CopyToAsync(memoryStream);
                        return (memoryStream.ToArray(), attachment.FileType);
                            
                    
                    }
            }
            }
        }

    }

    
}
