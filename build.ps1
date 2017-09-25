dotnet restore;
dotnet build -c Release;
dotnet pack --no-build -c Release src/microscopic/microscopic.csproj;

$nupkg = (Get-ChildItem src/microscopic/bin/Release/*.nupkg)[0];

# Push the nuget package to AppVeyor's artifact list.
Push-AppveyorArtifact $nupkg.FullName -FileName $nupkg.Name -DeploymentName "Microscopic.nupkg";