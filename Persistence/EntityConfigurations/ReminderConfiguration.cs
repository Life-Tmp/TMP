using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;
using TMPDomain.Entities;

namespace Persistence.EntityConfigurations
{
    public class ReminderConfiguration : IEntityTypeConfiguration<Reminder>
    {
        public void Configure(EntityTypeBuilder<Reminder> builder)
        {
            builder.HasKey(t => t.Id);

            builder.HasOne(r => r.Task)
    .WithMany(t => t.Reminders)
    .HasForeignKey(r => r.TaskId);
        }
    }
}
