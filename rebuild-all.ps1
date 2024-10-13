# Clean builds
dotnet clean .\SliceA
dotnet clean .\SliceB

# Remove the local cache
$cacheFolder = (${env:USERPROFILE} ?? ${env:HOME}) + "\.nuget\packages\"
Remove-Item -Force  "$cacheFolder\slicea.database\**\*.*"

# Build packages
mkdir .\NugetFeed
dotnet pack -o .\NugetFeed .\SliceA\    
Copy-Item .\SliceA\**\*.nupkg .\NugetFeed

# Build SliceB
dotnet build .\SliceB
