param(
	[string]$Root = "Monolith-Games"
)
$wrapper = Join-Path $Root "gradlew.bat"
if (Test-Path $wrapper) { Write-Output "Gradle wrapper already exists at $wrapper"; exit 0 }
if (-not (Get-Command gradle -ErrorAction SilentlyContinue)) { Write-Output "Gradle not found in PATH. Install Gradle or run wrapper creation manually."; exit 1 }
Push-Location $Root
try {
	gradle wrapper
	Write-Output "Gradle wrapper created in $Root"
} catch {
	Write-Output "Failed to create Gradle wrapper: $_"
} finally { Pop-Location }
