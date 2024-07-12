using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TMPDomain.Entities;

namespace Persistence.EntityConfigurations
{
    public class TaskDurationConfiguration : IEntityTypeConfiguration<TaskDuration>
    {
        public void Configure(EntityTypeBuilder<TaskDuration> builder)
        {
            builder.HasKey(td => td.Id);

            builder.HasOne(td => td.Task)
                .WithMany(t => t.TaskDurations)
                .HasForeignKey(td => td.TaskId);

            builder.HasOne(td => td.User)
                .WithMany(u => u.TaskDurations)
                .HasForeignKey(td => td.UserId);
        }
    }
}
