using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TMPDomain.Entities;

public class SubtaskConfiguration : IEntityTypeConfiguration<Subtask>
{
    public void Configure(EntityTypeBuilder<Subtask> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Title)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(s => s.Description)
            .HasMaxLength(1000);

        builder.Property(s => s.CompletedAt)
            .IsRequired(false);

        builder.HasOne(s => s.Task)
            .WithMany(t => t.Subtasks)
            .HasForeignKey(s => s.TaskId);
    }
}
