using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace TMPDomain.Entities
{
    public class Attachment
    {
        [Key]
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public long FileSize { get; set; }
        public string FileType { get; set; }
        public DateTime UploadDate { get; set; }
        public int TaskId { get; set; }
        public Task Tasks { get; set; }
    }
}
