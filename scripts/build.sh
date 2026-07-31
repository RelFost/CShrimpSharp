#!/usr/bin/env bash
set -euo pipefail

repository_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$repository_root"

dotnet restore CShrimpSharp.sln
dotnet format CShrimpSharp.sln --verify-no-changes --no-restore
dotnet build CShrimpSharp.sln --configuration Release --no-restore
dotnet test CShrimpSharp.sln --configuration Release --no-build
dotnet run --project samples/CShrimpSharp.Example/CShrimpSharp.Example.csproj --configuration Release --no-build
dotnet pack src/CShrimpSharp/CShrimpSharp.csproj --configuration Release --no-build --output artifacts/packages
