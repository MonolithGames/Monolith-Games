$projects = @(
	'Monolith-Games\\games\\Monolith\\Monolith.csproj',
	'Monolith-Games\\web\\monolith-games-site\\Monolith.csproj',
	'Monolith-Games\\platform\\editor\\Monolith.Editor\\Monolith.Editor.csproj',
	'Monolith-Games\\platform\\launcher\\Monolith.Launcher\\Monolith.Launcher.csproj',
	'Monolith-Games\\platform\\publishing\\Monolith.Publish\\Monolith.Publish.csproj',
	'Monolith-Games\\platform\\shared\\Monolith.Sentinel\\Monolith.Sentinel.csproj',
	'Monolith-Games\\platform\\shared\\Monolith.Shared\\Monolith.Shared.csproj',
	'Monolith-Games\\platform\\store\\Monolith.Store\\Monolith.Store.csproj',
	'Monolith-Games\\web\\admin-dashboard\\Monolith.Store.csproj',
	'Monolith-Games\\platform\\telemetry\\Monolith.Telemetry\\Monolith.Telemetry.csproj',
	'Monolith-Games\\games\\Monolith\\Monolith.Web.csproj',
	'Monolith-Games\\web\\monolith-games-site\\Monolith.Web.csproj',
	'Monolith-Games\\platform\\wrapper\\MonolithWrapper\\MonolithWrapper.csproj',
	'Monolith-Games\\games\\VoidStrikersNet\\VoidStrikersNet.csproj'
)

foreach ($p in $projects) {
	$full = Resolve-Path $p -ErrorAction SilentlyContinue
	if (-not $full) {
		Write-Output "$p|MISSING|0|0"
		continue
	}
	$dir = Split-Path $full -Parent
	$files = Get-ChildItem -Path $dir -Recurse -Include *.cs,*.razor,*.cshtml,*.java,*.kt,*.xml,*.html,*.js,*.css -File -ErrorAction SilentlyContinue
	$count = 0
	foreach ($f in $files) {
		try { $count += (Get-Content $f -ErrorAction SilentlyContinue | Measure-Object -Line).Lines } catch {}
	}
	Write-Output "$($full.Path)|$dir|$($files.Count)|$count"
}
