using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SliceB.Infrastructure.Configuration;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customer", "SliceA");
        builder.HasKey(x => x.CustomerId);
        builder.Property(x => x.CustomerId).UseIdentityColumn();

        builder.Property(x => x.Name).HasMaxLength(250);
    }
}