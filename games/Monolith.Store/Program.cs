using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System.IO;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseStaticFiles();

string HTML = @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""utf-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <title>Monolith Store / Official Android Games</title>
    <style>
        @import url('https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700&family=JetBrains+Mono:wght@400;500&display=swap');
        body { font-family: 'Inter', sans-serif; background: #0a0a0a; color: #ffffff; margin: 0; padding: 0; color-scheme: dark; }
        .store-header { background: #0d0d0d; border-bottom: 1px solid #222222; padding: 1.25rem 2.5rem; display: flex; justify-content: space-between; align-items: center; }
        .store-brand { font-weight: 700; font-size: 1.15rem; letter-spacing: .05em; }
        .store-brand span { color: #888; font-weight: 400; font-size: .9rem; }
        .store-tagline { font: 500 .75rem 'JetBrains Mono', monospace; color: #888; }
        .store-content { padding: 3rem clamp(1.5rem, 5vw, 6rem); max-width: 1400px; margin: 0 auto; }
        .hero-section { margin-bottom: 3.5rem; }
        .hero-section h1 { font-size: clamp(2.5rem, 4vw, 4rem); font-weight: 700; letter-spacing: -0.02em; margin: 0 0 .75rem 0; }
        .hero-section p { color: #a1a1a1; font-size: 1.1rem; max-width: 600px; margin: 0; }
        .games-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(340px, 1fr)); gap: 1.5rem; }
        .game-card { background: #141414; border: 1px solid #262626; border-radius: 10px; padding: 1.75rem; display: flex; flex-direction: column; transition: border-color .2s ease, transform .2s ease; }
        .game-card:hover { border-color: #ffffff; transform: translateY(-2px); }
        .category-pill { font: 600 .68rem 'JetBrains Mono', monospace; letter-spacing: .08em; color: #a1a1a1; text-transform: uppercase; margin-bottom: .75rem; }
        .game-card h2 { font-size: 1.4rem; font-weight: 600; margin: 0 0 .5rem 0; }
        .game-card p { color: #a1a1a1; font-size: .92rem; line-height: 1.5; margin: 0 0 1.5rem 0; flex: 1; }
        .game-footer { display: flex; justify-content: space-between; align-items: center; border-top: 1px solid #222222; padding-top: 1.25rem; margin-top: auto; }
        .price { font: 600 .85rem 'JetBrains Mono', monospace; color: #4ade80; }
        .download-btn { background: #ffffff; color: #000000; padding: .5rem 1rem; border-radius: 6px; font-weight: 600; font-size: .85rem; text-decoration: none; transition: opacity .15s ease; }
        .download-btn:hover { opacity: .9; }
    </style>
</head>
<body>
    <header class=""store-header"">
        <div class=""store-brand"">MONOLITH STORE <span>// A+ Android Games</span></div>
        <div class=""store-tagline"">Official Digital Distribution Hub</div>
    </header>
    <main class=""store-content"">
        <section class=""hero-section"">
            <h1>Monolith App Store.</h1>
            <p>Download elite A+ Android games built by Monolith Games. Fully integrated with AdMob ads and Google Play In-App Purchases.</p>
        </section>
        <section class=""games-grid"">
            <div class=""game-card"">
                <span class=""category-pill"">Sci-Fi RPG</span>
                <h2>Aetheria: Void Protocol</h2>
                <p>High-speed space exploration and void combat with crystal currency.</p>
                <div class=""game-footer""><span class=""price"">Free (IAP)</span><a class=""download-btn"" href=""/download/aetheria"">Download APK -&gt;</a></div>
            </div>
            <div class=""game-card"">
                <span class=""category-pill"">Physics Puzzle</span>
                <h2>Kinetics: Zero</h2>
                <p>Precision physics puzzle platformer with gravity manipulation.</p>
                <div class=""game-footer""><span class=""price"">Free (Ad-Supported)</span><a class=""download-btn"" href=""/download/kinetics"">Download APK -&gt;</a></div>
            </div>
            <div class=""game-card"">
                <span class=""category-pill"">Tactical Strategy</span>
                <h2>Chronos Breach</h2>
                <p>Time-manipulation strategy game across alternate historical eras.</p>
                <div class=""game-footer""><span class=""price"">Free (IAP)</span><a class=""download-btn"" href=""/download/chronos"">Download APK -&gt;</a></div>
            </div>
            <div class=""game-card"">
                <span class=""category-pill"">Space Survival</span>
                <h2>Nebula Veil</h2>
                <p>Atmospheric deep space survival and stardust resource harvesting.</p>
                <div class=""game-footer""><span class=""price"">Free (IAP)</span><a class=""download-btn"" href=""/download/nebula"">Download APK -&gt;</a></div>
            </div>
            <div class=""game-card"">
                <span class=""category-pill"">Action Combat</span>
                <h2>Valkyrie Ascendant</h2>
                <p>Mythic Valhalla action combat with battle glory progression.</p>
                <div class=""game-footer""><span class=""price"">Free (IAP)</span><a class=""download-btn"" href=""/download/valkyrie"">Download APK -&gt;</a></div>
            </div>
            <div class=""game-card"">
                <span class=""category-pill"">Colony Builder</span>
                <h2>Shattered Horizon</h2>
                <p>Post-apocalyptic colony expansion and resource outpost management.</p>
                <div class=""game-footer""><span class=""price"">Free (IAP)</span><a class=""download-btn"" href=""/download/shattered"">Download APK -&gt;</a></div>
            </div>
            <div class=""game-card"">
                <span class=""category-pill"">Cyberpunk Stealth</span>
                <h2>Phantom Grid</h2>
                <p>Tactical cyber-infiltration and node hacking puzzle game.</p>
                <div class=""game-footer""><span class=""price"">Free (IAP)</span><a class=""download-btn"" href=""/download/phantom"">Download APK -&gt;</a></div>
            </div>
            <div class=""game-card"">
                <span class=""category-pill"">Solar Racing</span>
                <h2>Solaris Drift</h2>
                <p>High-velocity solar flare racing and engine thermal management.</p>
                <div class=""game-footer""><span class=""price"">Free (IAP)</span><a class=""download-btn"" href=""/download/solaris"">Download APK -&gt;</a></div>
            </div>
            <div class=""game-card"">
                <span class=""category-pill"">Deep Sea Expl.</span>
                <h2>Abyssal Echo</h2>
                <p>Sonar submarine deep-sea trench exploration and artifact retrieval.</p>
                <div class=""game-footer""><span class=""price"">Free (IAP)</span><a class=""download-btn"" href=""/download/abyssal"">Download APK -&gt;</a></div>
            </div>
            <div class=""game-card"">
                <span class=""category-pill"">Grand Strategy</span>
                <h2>Apex Dominion</h2>
                <p>Empire building, province conquering, and tribute collection.</p>
                <div class=""game-footer""><span class=""price"">Free (IAP)</span><a class=""download-btn"" href=""/download/apex"">Download APK -&gt;</a></div>
            </div>
        </section>
    </main>
</body>
</html>";

app.MapGet("/", () => Results.Content(HTML, "text/html"));

app.MapGet("/download/{game}", (string game, IHostEnvironment env) =>
{
    string fileName = $"{game.ToLowerInvariant()}.apk";
    string[] candidatePaths =
    [
        Path.Combine(AppContext.BaseDirectory, "wwwroot", "downloads", fileName),
        Path.Combine(env.ContentRootPath, "wwwroot", "downloads", fileName),
        Path.Combine("C:/Users/auora/StudioProjects/Monolith-Games/src/Monolith.Store/wwwroot/downloads", fileName)
    ];

    foreach (var path in candidatePaths)
    {
        if (File.Exists(path))
        {
            return Results.File(path, contentType: "application/vnd.android.package-archive", fileDownloadName: $"{game}.apk");
        }
    }

    return Results.NotFound($"APK for game '{game}' not found in candidate paths.");
});

app.Run();
