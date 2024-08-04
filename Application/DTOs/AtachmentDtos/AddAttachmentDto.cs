using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMPApplication.DTOs.AtachmentDtos
{
    public class AddAttachmentDto
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public long FileSize { get; set; }
        public string FileType { get; set; }
        public int TaskId { get; set; }
        public int AttachmentId {  get; set; }
    }
}
