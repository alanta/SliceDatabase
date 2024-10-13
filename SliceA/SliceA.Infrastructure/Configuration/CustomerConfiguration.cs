using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SliceA.Infrastructure.Configuration;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customer");
        builder.HasKey(x => x.CustomerId).HasName("PK_SliceA_Customer");
        builder.Property(x => x.CustomerId).UseIdentityColumn();

        builder.Property(x => x.Name).HasMaxLength(250);
    }
}