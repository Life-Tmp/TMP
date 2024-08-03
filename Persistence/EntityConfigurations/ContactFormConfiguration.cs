using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TMPDomain.Entities;

namespace Persistence.EntityConfigurations
{
    public class ContactFormConfiguration : IEntityTypeConfiguration<ContactForm>
    {
        public void Configure(EntityTypeBuilder<ContactForm> builder)
        {
            builder.HasKey(cf => cf.Id);

            builder.Property(cf => cf.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(cf => cf.LastName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(cf => cf.Email)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(cf => cf.PhoneNumber)
                .HasMaxLength(50);

            builder.Property(cf => cf.Message)
                .IsRequired();

            builder.Property(cf => cf.ResponseMessage)
                .HasMaxLength(2000);

            builder.Property(cf => cf.RespondedAt);

            builder.Property(cf => cf.CreatedAt)
                .IsRequired();
        }
    }
}
