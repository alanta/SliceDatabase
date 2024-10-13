using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SliceA.Infrastructure.Configuration;

public class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> builder)
    {
        builder.ToTable("Contact");
        builder.HasKey(x => x.ContactId).HasName("PK_SliceA_Contact");
        builder.Property(x => x.ContactId).UseIdentityColumn();

        builder.Property(x => x.FirstName).HasMaxLength(250);
        builder.Property(x => x.LastName).HasMaxLength(250);
    }
}