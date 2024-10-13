using Microsoft.EntityFrameworkCore;
using SliceB.Infrastructure.Configuration;

namespace SliceB.Infrastructure;

public partial class SliceDataContext : DbContext
{
    public SliceDataContext()
    {
    }

    public SliceDataContext(DbContextOptions<SliceDataContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("SliceB");

        modelBuilder.ApplyConfiguration(new ContactConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}