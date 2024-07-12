using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace TMPDomain.Entities
{
    public class Attachment
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public int TaskId { get; set; }
        public DateTime UploadedAt { get; set; }
        public Task Task { get; set; }
    }
}
