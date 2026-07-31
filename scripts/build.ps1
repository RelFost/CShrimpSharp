$ErrorActionPreference = 'Stop'
dotnet restore CShrimpSharp.sln
dotnet format CShrimpSharp.sln --verify-no-changes --no-restore
dotnet build CShrimpSharp.sln -c Release --no-restore
dotnet test CShrimpSharp.sln -c Release --no-build
dotnet pack src/CShrimpSharp/CShrimpSharp.csproj -c Release --no-build -o artifacts/packages
