.PHONY: build test run doctor android-build android-clean

build:
	python3 tools/monolith/monolith.py build all

test:
	dotnet test Monolith.sln

run:
	dotnet run --project src/Monolith/Monolith.csproj -- --urls http://localhost:65000

doctor:
	python3 tools/monolith/monolith.py doctor

android-build:
	gradle assembleDebug

android-clean:
	gradle clean
