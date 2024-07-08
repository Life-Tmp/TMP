
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Task = TMPDomain.Entities.Task;

namespace TMP.Persistence.EntityConfigurations
{
    public class TaskConfiguration : IEntityTypeConfiguration<Task>
    {

        public void Configure(EntityTypeBuilder<Task> builder)
        {
            builder.HasKey(t => t.Id);
            builder.HasIndex(t => t.Title);
            builder.HasIndex(t => t.Priority);
            builder.HasIndex(t => t.Status);
            builder.HasIndex(t => t.CreatedAt);
            builder.HasIndex(t => t.DueDate);
            builder.HasIndex(t => t.ProjectId);
        }
    }
}
