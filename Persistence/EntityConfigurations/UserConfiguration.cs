using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TMPDomain.Entities;

namespace Persistence.EntityConfigurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.Id);

            builder.Property(u => u.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.LastName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(u => u.PasswordHash)
                .IsRequired();

            builder.HasMany(u => u.ProjectsCreated)
                .WithOne(p => p.CreatedByUser)
                .HasForeignKey(p => p.CreatedByUserId);

            builder.HasMany(u => u.Comments)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId);

            builder.HasMany(u => u.Notifications)
                .WithOne(n => n.User)
                .HasForeignKey(n => n.UserId);

            builder.HasMany(u => u.TaskDurations)
                .WithOne(td => td.User)
                .HasForeignKey(td => td.UserId);

            builder.HasMany(u => u.AssignedTasks)
                .WithMany(t => t.AssignedUsers);
        }
    }
}
