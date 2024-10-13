<#
 .SYNOPSIS
    Run the database in a container

 .DESCRIPTION
    This starts a SQL Server 2022 container and creates the database in it.
    A docker volume is mounted to make the data persistent.

 .EXAMPLE
    ./run-sql.ps1

 .PARAMETER Clean
    Defaults to false, which retains the data container. Set to $true to clear existing data and
    rebuild the database from scratch. 
    For example: ./run-sql.ps1 -Clean $true
#>

param(
 [bool]
 $Clean=$false
)

$databaseName="Domain"
$containerName="${databaseName}sql"
$volumeName="${databaseName}data"
# Set the database project name
$databaseProject="SliceA.Database"
$databaseProjectPath="$PWD/../$databaseProject"

# List the dacpacs to deploy
$dacpacs=@(
  "$databaseProject.dacpac"
)

$ErrorActionPreference = "Stop"

$SAPassword="Slice&Dice!"

# set working dir to script folder
$scriptpath = $MyInvocation.MyCommand.Path
$dir = Split-Path $scriptpath
$sqlContainerExists = $false;

Push-Location $dir

if( (docker ps -a | Select-String -Pattern "$containerName" -SimpleMatch ) )
{
   if( $Clean )
   {
     Write-Warning "Removing existing container"
     docker container rm --force $containerName
     if ($LastExitCode -ne 0) { Write-Error "Failed to delete container" }
   }
   else {
    $sqlContainerExists = $true
    Write-Host -ForegroundColor Green "Using already existing container named $containerName. "  
   }
}

if( (docker volume list | Select-String -Pattern "$volumeName" -SimpleMatch ) )
{
  if( ($Clean) )
  {
    Write-Warning "Removing existing data volume"
    docker volume rm --force $volumeName
    if ($LastExitCode -ne 0) { Write-Error "Failed to delete data container" }
  }
  else{
    Write-Host -ForegroundColor Green "Using existing data container"
  }
}

Write-Host -ForegroundColor Green 'Run SQL Server 2022 container'
if( $sqlContainerExists )
{
  docker start $containerName
}
else {
  docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=$SAPassword" -p 1433:1433 --name $containerName -d -u 0:0 -v "${volumeName}:/var/opt/mssql" mcr.microsoft.com/mssql/server:2022-latest   
}
if ($LastExitCode -ne 0) { Write-Error "Failed to run container" }

Write-Host -ForegroundColor Green 'Enabled contained databases'
docker exec -ti $containerName /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SAPassword" -C -Q "sp_configure 'contained database authentication', 1;`
	GO `
	RECONFIGURE; `
	GO"
if ($LastExitCode -ne 0) { Write-Error "Failed to reconfigure database server" }

Write-Host -ForegroundColor Green 'Build database (dacpac)'
dotnet build "${databaseProjectPath}"
if ($LastExitCode -ne 0) { Write-Error "Failed to build database" }

if(!(Test-Path -Path '.\sqlpackage\sqlpackage.exe')) {
  Write-Host -ForegroundColor Green 'Setup SQL Package tool'
  
  dotnet tool update --tool-path sqlpackage microsoft.sqlpackage
  
  if ($LastExitCode -ne 0) { Write-Error "Failed to install sqlpackage" }
}

Write-Host -ForegroundColor Green 'Deploy database'
$ConnectionString = "Data Source=.;Initial Catalog=$databaseName;Persist Security Info=True;User ID=sa;Password=$SAPassword;Connection Timeout=30;TrustServerCertificate=true"
foreach ($dacpac in $dacpacs) {
  Write-Host -ForegroundColor Green "Deploy $dacpac"
  ./sqlpackage/sqlpackage /Action:Publish /TargetConnectionString:"$ConnectionString" /SourceFile:"${databaseProjectPath}/bin/Debug/netstandard2.1/${dacpac}" /p:DropObjectsNotInSource=False /p:BlockOnPossibleDataLoss=False
}
if ($LastExitCode -ne 0) { Write-Error "Failed to migrate database" }


Pop-Location