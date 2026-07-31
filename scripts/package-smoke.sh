#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PACKAGE_DIR="$ROOT/artifacts/packages"
SMOKE_DIR="$ROOT/artifacts/package-smoke"
rm -rf "$SMOKE_DIR"
mkdir -p "$SMOKE_DIR"
dotnet new console --framework net8.0 --output "$SMOKE_DIR" --force
dotnet add "$SMOKE_DIR/package-smoke.csproj" package CShrimpSharp --version 0.3.0-preview.1 --source "$PACKAGE_DIR"
cat > "$SMOKE_DIR/Program.cs" <<'CS'
using CShrimpSharp;
Result<int, Failure> result = Result.Success(21).Map(static value => value * 2);
return result.Value == 42 ? 0 : 1;
CS
dotnet run --project "$SMOKE_DIR/package-smoke.csproj" --configuration Release
