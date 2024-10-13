using EfSchemaCompare;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SliceA.Infrastructure;

namespace SliceA.Tests;

[Collection(DatabaseCollection.Name)]
public class When_loading_data
{
    private readonly TestDatabase _database;

    public When_loading_data(TestDatabase database)
    {
        _database = database;
    }

    [Fact]
    public async Task The_schema_should_match_the_datamodel()
    {
        // Arrange
        await using var context = CreateDbContext();
        var comparer = new CompareEfSql();

        // Act
        bool hasErrors = comparer.CompareEfWithDb(_database.DatabaseConnectionString, context);

        // Assert
        hasErrors.Should().BeFalse(comparer.GetAllErrors);
    }

    [Fact]
    public async Task It_should_load_contacts()
    {
        // Arrange
        await using (var seedCtx = CreateDbContext())
        {
            var microsoft = new Customer
            {
                Name = "Microsoft"
            };

            seedCtx.Add(new Contact { FirstName = "Chris", LastName = "Sharp", Customer = microsoft });
            await seedCtx.SaveChangesAsync();
        }

        using var ctx = CreateDbContext();

        // Act
        var contacts = await ctx.Set<Contact>().Include(x => x.Customer).ToArrayAsync();

        // Assert
        contacts.Should().NotBeEmpty();
    }
    public SliceDataContext CreateDbContext()
    {
        return new SliceDataContext(new DbContextOptionsBuilder<SliceDataContext>()
            .UseSqlServer(_database.DatabaseConnectionString)
            .Options);
    }
}