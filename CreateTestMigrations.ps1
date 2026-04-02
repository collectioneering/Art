$ErrorActionPreference = 'Stop'
dotnet tool update dotnet-ef -g --version 9.0.14
Remove-Item -Path "$PSScriptRoot/src/Art.EF.Sqlite.Tests.Migrations1/Migrations" -Recurse
Remove-Item -Path "$PSScriptRoot/src/Art.EF.Sqlite.Tests.Migrations2/Migrations" -Recurse
dotnet ef migrations add --project "$PSScriptRoot/src/Art.EF.Sqlite.Tests.Migrations1" InitialCreate
Copy-Item -Path "$PSScriptRoot/src/Art.EF.Sqlite.Tests.Migrations1/Migrations" -Destination "$PSScriptRoot/src/Art.EF.Sqlite.Tests.Migrations2/Migrations" -Recurse
dotnet ef migrations add --project "$PSScriptRoot/src/Art.EF.Sqlite.Tests.Migrations2" DummyMigrationQX1
