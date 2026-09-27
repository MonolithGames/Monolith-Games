$modules = Get-ChildItem -Recurse -Include build.gradle.kts -File -ErrorAction SilentlyContinue | ForEach-Object { [PSCustomObject]@{ Path = $_.DirectoryName; BuildFile = $_.FullName } } | Sort-Object Path | Select-Object -Unique
$modules | ConvertTo-Json -Depth 3 | Out-File -Encoding UTF8 Monolith-Games/scripts/android_modules.json
Write-Output "Wrote Monolith-Games/scripts/android_modules.json with $($modules.Count) entries"
