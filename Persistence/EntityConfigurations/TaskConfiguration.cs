﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TMPDomain.Entities;
using Task = TMPDomain.Entities.Task;

namespace Persistence.EntityConfigurations
{
    public class TaskConfiguration : IEntityTypeConfiguration<Task>
    {
        public void Configure(EntityTypeBuilder<Task> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Title)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(t => t.Description)
                .HasMaxLength(1000);

            builder.HasOne(t => t.Project)
                .WithMany(p => p.Tasks)
                .HasForeignKey(t => t.ProjectId);

            builder.HasMany(t => t.Comments)
                .WithOne(c => c.Task)
                .HasForeignKey(c => c.TaskId);

            builder.HasMany(t => t.Attachments)
                .WithOne(a => a.Task)
                .HasForeignKey(a => a.TaskId);

            builder.HasMany(t => t.TaskDurations)
                .WithOne(td => td.Task)
                .HasForeignKey(td => td.TaskId);

            builder.HasMany(t => t.AssignedUsers)
                .WithMany(u => u.AssignedTasks);

            builder.HasMany(t => t.Tags)
                .WithMany(tag => tag.Tasks) 
                .UsingEntity(j => j.ToTable("TaskTags"));
        }
    }
}
