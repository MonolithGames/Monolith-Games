$roots = Get-ChildItem -Recurse -Include build.gradle.kts -File -ErrorAction SilentlyContinue | Select-Object -ExpandProperty DirectoryName -Unique
foreach ($dir in $roots) {
	$files = Get-ChildItem -Path $dir -Recurse -Include *.kt,*.java,*.xml,*.gradle,*.kts,*.js,*.html,*.css -File -ErrorAction SilentlyContinue
	$lines = 0
	foreach ($f in $files) {
		try { $lines += (Get-Content $f -ErrorAction SilentlyContinue | Measure-Object -Line).Lines } catch {}
	}
	Write-Output "$dir|$($files.Count)|$lines"
}
