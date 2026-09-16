#!/usr/bin/env sh
set -eu
exec dotnet publish web/Monolith.Web/Monolith.Web.csproj --configuration Release --output "${1:-artifacts/monolith}"
