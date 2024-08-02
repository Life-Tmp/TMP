using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMPApplication.DTOs.AtachmentDtos
{
    public class FileAttachmentDto
    {
        public byte[] FileBytes { get; set; }
        public string FileType { get; set; }
        public string FileName { get; set; }
    }
}
