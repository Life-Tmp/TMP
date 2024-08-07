﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPApplication.DTOs.AtachmentDtos;
using TMPDomain.Entities;

namespace TMPApplication.Interfaces.AttachmentTasks
{
    public interface IAttachmentService
    {
        Task<AddAttachmentDto> UploadAttachmentAsync(IFormFile file, int taskId);
        Task<bool> RemoveAttachmentAsync(int attachmentId);
        Task<FileAttachmentDto> DownloadAttachmentAsync(int attachmentId);
        Task<List<Attachment>> GetAttachmentsAsync(int taskId);
    }
}
