using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TMPDomain.Entities;

namespace Persistence.EntityConfigurations
{
    public class TagConfiguration : IEntityTypeConfiguration<Tag>
    {
        public void Configure(EntityTypeBuilder<Tag> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(t => t.Color)
                .HasMaxLength(7); // For storing color codes like #FFFFFF

            builder.HasMany(t => t.Tasks)
                .WithMany(t => t.Tags)
                .UsingEntity(j => j.ToTable("TaskTags"));
        }
    }
}
