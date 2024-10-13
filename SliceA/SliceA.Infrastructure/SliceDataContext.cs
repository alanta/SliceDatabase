using Microsoft.EntityFrameworkCore;
using SliceA.Infrastructure.Configuration;

namespace SliceA.Infrastructure;

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
        modelBuilder.HasDefaultSchema("SliceA");

        modelBuilder.ApplyConfiguration(new ContactConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}