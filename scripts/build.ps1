$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repositoryRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
Push-Location $repositoryRoot

try {
    dotnet restore CShrimpSharp.sln
    if ($LASTEXITCODE -ne 0) { throw 'dotnet restore failed.' }

    dotnet format CShrimpSharp.sln --verify-no-changes --no-restore
    if ($LASTEXITCODE -ne 0) { throw 'dotnet format verification failed.' }

    dotnet build CShrimpSharp.sln --configuration Release --no-restore
    if ($LASTEXITCODE -ne 0) { throw 'dotnet build failed.' }

    dotnet test CShrimpSharp.sln --configuration Release --no-build
    if ($LASTEXITCODE -ne 0) { throw 'dotnet test failed.' }

    dotnet run --project samples/CShrimpSharp.Example/CShrimpSharp.Example.csproj --configuration Release --no-build
    if ($LASTEXITCODE -ne 0) { throw 'Example application failed.' }

    dotnet pack src/CShrimpSharp/CShrimpSharp.csproj --configuration Release --no-build --output artifacts/packages
    if ($LASTEXITCODE -ne 0) { throw 'dotnet pack failed.' }
}
finally {
    Pop-Location
}
