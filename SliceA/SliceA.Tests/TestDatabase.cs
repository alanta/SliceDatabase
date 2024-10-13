using System.Data;
using Testcontainers.MsSql;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Dac;

namespace SliceA.Tests;
public class TestDatabase : IAsyncLifetime
{
    public MsSqlContainer MsSqlContainer { get; }

    public string DatabaseConnectionString { get; private set; } = "";

    public TestDatabase()
    {
        MsSqlContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword( "yourStrong(!)Password")
            .Build();
    }

    public async Task InitializeAsync()
    {
        if (!string.IsNullOrWhiteSpace(DatabaseConnectionString))
        {
            return;
        }

        await MsSqlContainer.StartAsync().ConfigureAwait(false);

        DatabaseConnectionString = MsSqlContainer.GetConnectionString();

        await InitializeDb();
    }

    public Task DisposeAsync()
    {
        return MsSqlContainer.DisposeAsync().AsTask();
    }

    private async Task InitializeDb()
    {
        var builder = new SqlConnectionStringBuilder(DatabaseConnectionString);
        builder.TrustServerCertificate = true;
        DatabaseConnectionString = builder.ToString();

        var dacServices = new DacServices(DatabaseConnectionString);
        using var dacpac1 = DacPackage.Load("SliceA.Database.dacpac");
        dacServices.Deploy(dacpac1, builder.InitialCatalog, true, new DacDeployOptions { BlockOnPossibleDataLoss = false, }, CancellationToken.None);

        //await ExecuteFiles(Path.GetFullPath("./SQL"));
    }

    public async Task ExecuteFiles(string pathToSqlFiles)
    {
        // These are the SQL scripts, most of these are in a specific order
        // so dependent items are created after the entities they depend on
        var sqlFilesInOrder = new[]
            {
                "Schema/SliceA.sql",
                "Tables/Customer.sql",
                "Tables/Contact.sql"

                // WARNING: You need to keep this in sync with your database project!

                // Add seed scripts at the end
            };

        var files = sqlFilesInOrder.Select( f =>  Path.Combine(pathToSqlFiles, f)).ToArray();
        
        await ExecuteFiles(files);
    }

    public async Task ExecuteFiles(params string[] sqlFileNames)
    {
        foreach (var file in sqlFileNames)
        {
            try
            {
                File.Exists(file).Should().BeTrue($"Missing file! : {file}");
                var batches = (await File.ReadAllTextAsync(file))
                        .Split("GO", StringSplitOptions.RemoveEmptyEntries);
                foreach (var batch in batches)
                {
                    await Execute(DatabaseConnectionString, batch);
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine(ex.Message);
                throw new DataException($"Error Executing '{file}'.", ex);
            }
        }
    }

    private static async Task Execute(string connectionString, string query)
    {
        await using var connection = new SqlConnection(connectionString);
        connection.Open();
        var cmd = new SqlCommand(query, connection)
        {
            CommandType = CommandType.Text
        };
        await cmd.ExecuteNonQueryAsync();
    }

}

[CollectionDefinition(DatabaseCollection.Name, DisableParallelization = true)]
public class DatabaseCollection : ICollectionFixture<TestDatabase>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
    public const string Name = "Database test";
}