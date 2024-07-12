using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TMPDomain.Entities;

namespace Persistence.EntityConfigurations
{
    public class ProjectConfiguration : IEntityTypeConfiguration<Project>
    {
        public void Configure(EntityTypeBuilder<Project> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(p => p.Description)
                .HasMaxLength(1000);

            builder.HasOne(p => p.CreatedByUser)
                .WithMany(u => u.ProjectsCreated)
                .HasForeignKey(p => p.CreatedByUserId);

            builder.HasMany(p => p.Tasks)
                .WithOne(t => t.Project)
                .HasForeignKey(t => t.ProjectId);

            builder.HasMany(p => p.Users)
                .WithMany(u => u.Projects)
                .UsingEntity(j => j.ToTable("ProjectUsers"));
        }
    }
}
