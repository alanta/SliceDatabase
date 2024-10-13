# How To Setup the database for a Slice or Module


This document describes how to setup the database project for individual slices.

## Prerequisites

Required tools:

* Dotnet 8 SDK
* Docker

Recommended tools:

* Visual Studio 2022 Professional or Enterprise
* SQL Server Management Studio

## Adding a database project

First, we need to create a database project.
To add a database project either use the templates or create a project from scratch.

### Using templates

If you haven't done so, install or update the project templates.

```pwsh
dotnet new install MSBuild.Sdk.SqlProj.Templates
```

Now, add a new database project:

```powershell
dotnet new sqlproj -s SqlAzure -n MySlice.Database
```

### From scratch

Add a new C# project and edit it to look like this:

```XML
<Project Sdk="MSBuild.Sdk.SqlProj/2.9.1">
    <PropertyGroup>
        <TargetFramework>netstandard2.1</TargetFramework>
        <SqlServerVersion>SqlAzure</SqlServerVersion>
        <!-- For additional properties that can be set here, please refer to https://github.com/rr-wfm/MSBuild.Sdk.SqlProj#model-properties -->
    </PropertyGroup>

    <PropertyGroup>
        <!-- Refer to https://github.com/rr-wfm/MSBuild.Sdk.SqlProj#publishing-support for supported publishing options -->
        <BlockOnPossibleDataLoss>True</BlockOnPossibleDataLoss>
    </PropertyGroup>
</Project>
```
## Project structure
The newly created database project needs to have a structure.
Structure your database project `MySlice.Database` like this:

* 📁 Schema
  * 📄 MySlice.sql
* 📁 Tables
  * 📄 TableOne.sql
  * 📄 TableTwo.sql
* 📁 Views
  * 📄 List.sql

Building this project produces a `dacpac` file with the database schema, that can be used for deployment, testing and integration with other slices.


## Testing the database

To run tests against the schema, we use testcontainers. This allows us to run the database in a container and verify the _real_ deployed database, not a mock.
To use the slice database schema in a test, add a project reference and tweak the reference:
```xml
<ProjectReference Include="..\SliceA.Database\SliceA.Database.csproj" ReferenceOutputAssembly="False" />
```

⚠ Set `ReferenceOutputAssembly="False"` to ensure the compiler doesn't try to load the dacpac as an assembly.

Adding the project reference ensures the dacpac builds and is available in the test. 

Next, when the test container starts we can use the dacpac to create the schema. The dacpac tooling is available as a nuget package:

* Add a package reference to [Microsoft.SqlServer.DacFx](https://www.nuget.org/packages/Microsoft.SqlServer.DacFx)

Now, when the database testcontainer starts you can provision the schema using the dacpac:

```csharp
private void InitializeDb()
{
  var dacServices = new DacServices(DatabaseConnectionString);
  using (var dacpac1 = DacPackage.Load("SliceA.Database.dacpac"))
  {
    dacServices.Deploy(dacpac1, builder.InitialCatalog, true, new DacDeployOptions { BlockOnPossibleDataLoss = false, }, CancellationToken.None);
  }
}
```

## Schema validation

One of the nice benefits of this setup is that we can now validate our schema against our EntityFramework model. There is a package available specifically designed for this: [EfCore.SchemaCompare](https://www.nuget.org/packages/EfCore.SchemaCompare).

* Add a reference to [EfCore.SchemaCompare](https://www.nuget.org/packages/EfCore.SchemaCompare)
* Setup the following test

```csharp
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

    public SliceDataContext CreateDbContext()
    {
        return new SliceDataContext(new DbContextOptionsBuilder<SliceDataContext>()
            .UseSqlServer(_database.DatabaseConnectionString)
            .Options);
    }
}
```

### Possible errors in test
`NOT IN DATABASE: MyTable->Index 'Column1', index constraint name. Expected = IX_MyTable_Column1`
This is because EF enforces that an index must be defined for each foreign key column.

`DIFFERENT: MyTable->Property 'MyTableId', value generated. Expected = OnAdd, found = Never`
This is probably because MyTableId is defined in EF as primary key, but in the SQL create table script "IDENTITY" is missing at the primary key column.


# Reference the schema of another slice

To reference the schema of another slice, add a Nuget package reference to the Nuget package published for that slice.

> ⚠ Slices are allowed to read each others data but NOT modify it.

## Reference the schema of another slices for testing

If a slices uses the schema of another slice, it needs to be pulled into the test project as well. The test project can reference the same Nuget package used in the SQL project.

* Add the nuget package for the other slices to the test project

This will ensure the dacpac is available and it can be used just like the dacpac for the slice itself:

```csharp
private void InitializeDb()
{
    var dacServices = new DacServices(DatabaseConnectionString);
    // Deploy each dacpac
    using (var dacpac1 = DacPackage.Load("SliceA.Database.dacpac"))
    {
        dacServices.Deploy(dacpac1, builder.InitialCatalog, true, new DacDeployOptions { BlockOnPossibleDataLoss = false, }, CancellationToken.None);
    }
    using (var dacpac2 = DacPackage.Load("SliceB.Database.dacpac"))
    {
        dacServices.Deploy(dacpac2, builder.InitialCatalog, true, new DacDeployOptions { BlockOnPossibleDataLoss = false }, CancellationToken.None);
    }
}
```

# Publish the schema as a Nuget package

If you want other slices to be able to reference entities in your slice's schema, the schema dacpac must be published in a Nuget package.
To enable this, add a nuspec file like this:

```xml
<?xml version="1.0"?>
<package >
  <metadata>
    <id>$id$</id>
    <version>$version$</version>
    <description>$description$</description>
    <authors>$authors$</authors>
    <owners>$authors$</owners>
    <copyright>$copyright$</copyright>
    <projectUrl>$projecturl$</projectUrl>
    <tags>$tags$</tags>
    <requireLicenseAcceptance>false</requireLicenseAcceptance>
    <repository type="git" url="https://github.com/myorg/SliceA" /> <!-- Set a link to the repo -->
  </metadata>
  <files>
    <!-- SQL projects need the dacpac in the tools folder -->
    <file src="bin\$configuration$\$tfm$\*.dacpac" target="tools/" />
    <!-- Test projects need the dacpac in the conent folder -->
    <file src="bin\$configuration$\$tfm$\*.dacpac" target="content/" />
   </files>
</package>
```
This file is mostly generic but make sure you update the repository url.

> ℹ The generated package unfortunately contains the same dacpac in 2 locations. This is needed to make the package work in all circumstances.

# Setup the database script for local development

Copy the database script from [SliceB/Scripts/run-sql.ps1](../SliceB/Scripts/run-sql.ps1) and update the module name and the list of dacpacs to be deployed.