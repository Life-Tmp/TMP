using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPDomain.Entities;

namespace TMP.Persistence.EntityConfigurations
{
    public class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
    {
        public void Configure(EntityTypeBuilder<Attachment> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.FileName);
            builder.HasIndex(x => x.TaskId); // INFO: indexing requires more storage
        }
    }
}
