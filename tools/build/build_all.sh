#!/usr/bin/env sh
set -eu

dotnet build web/Monolith.Web/Monolith.Web.csproj
dotnet test web/Monolith.Web.Tests/Monolith.Web.Tests.csproj
