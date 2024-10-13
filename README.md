# Slice databases

This project demonstrates how to manage multiple schemas in a single database.

The goal is to support modular monoliths and/or vertical slice architecture in dotnet with SQL Server.
Though the project use EntityFramework, it uses dacpacs to deploy changes to the database.

The data models are managed using [MSBuild.Sdk.SqlProj](https://github.com/rr-wfm/MSBuild.Sdk.SqlProj), a cross-platform implementation of SQL projects for Dotnet.

The main requirements are:

* Database project per slice / module to manage the schema using SQL `CREATE` scripts
* Each slice / module is completely independent. It can have it's own repository and be deployed (mostly) independent.
* Each slice / module owns it's own database schema
* Reading data from another slices is possible by querying the schema of that module
* Testing slices is possible, both in local development and CI/CD scenarios
* Building and testing slices with a dependency on another slices' database schema is possible

🚨 TODO
* Prevent EF from inserting or updating data into another schema
* Define a convention for sharing models and EF mappings between slices

## How does it work

![Domain database with slices.export.svg](./Docs/Domain%20database%20with%20slices.svg)

Each slice publishes it's schema as a dacpac for deployment and as a Nuget package. Other slices can pull that package in to setup a test database using the dacpac to ensure the database is up to date.

This works with both command-line scripts and test containers to create complete and up-to-date databases on the fly.

## What's in this repository

This repository hosts to slices: SliceA and SliceB. SliceA had `Customers` and `Contacts`, SliceB has `Orders` that reference the customers and contacts in SliceA.

The schema for SliceA is made available through a Nuget package. So SliceB can use it to build a database for tests. 

Currently SliceB redefines tables it needs from SliceA in the EF DataContext, but you could share these configurations if needed by importing the SliceA.Infrastryucture package.

Please read the detailed [How To](Docs/HOWTO.md) for instructions on how to setup new slices/modules.

## Requirements
* [Dotnet 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) on Linux or Windows (MacOS is not tested, but should work)
* Docker for running SQL Server for development and testing
* [Powershell 7+](https://learn.microsoft.com/en-us/powershell/scripting/install/installing-powershell)

## Please note
To be able to test nuget integration between SliceA and SliceB this repository uses a local Nuget feed in a the [NugetFeed](./NugetFeed/) folder.

Nuget packages are cached, and we're not versioning packages within this repository so you MUST purge the cache an do a clean rebuild to get updated packages from SliceA in SliceB.

In a real-life scenario the slices would each live in their own repository and use a private Nuget-feed.

## Build everything

Use [rebuild-all.ps1](rebuild-all.ps1) to clean the local Nuget cache and rebuild both slices using the local NugetFeed folder.

## Run the tests

Change into `SliceA` or `SliceB` and run `dotnet test`.

Note that you need to run [rebuild-all.ps1](rebuild-all.ps1) to ensure the package for SliceA is available before you can buils SliceB.

> ℹ️ Running the tests for the first time may take some time if the SQL Server container image is not available on the system yet.

## Setup development database

Use [SliceA/Scripts/run-sql.ps1](./SliceA/Scripts/run-sql.ps1) to setup a local SQL Server container for development of `SliceA`.

Use [SliceB/Scripts/run-sql.ps1](./SliceA/Scripts/run-sql.ps1) to setup a local SQL Server container for development of `SliceB`. Since SliceB depends on SliceA, it will also apply the dacpac for SliceA to the database.