.PHONY: build test run

build:
	dotnet build web/Monolith.Web/Monolith.Web.csproj

test:
	dotnet test web/Monolith.Web.Tests/Monolith.Web.Tests.csproj

run:
	dotnet run --project web/Monolith.Web/Monolith.Web.csproj
