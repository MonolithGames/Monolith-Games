.PHONY: build test run doctor

build:
	python3 tools/monolith/monolith.py build all

test:
	dotnet test web/Monolith.Web.Tests/Monolith.Web.Tests.csproj

run:
	dotnet run --project web/Monolith.Web/Monolith.Web.csproj

doctor:
	python3 tools/monolith/monolith.py doctor
