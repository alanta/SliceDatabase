using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SliceB.Infrastructure.Configuration;

public class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> builder)
    {
        builder.ToTable("Contact","SliceA");
        builder.HasKey(x => x.ContactId);
        builder.Property(x => x.ContactId).UseIdentityColumn();

        builder.Property(x => x.FirstName).HasMaxLength(250);
        builder.Property(x => x.LastName).HasMaxLength(250);
    }
}