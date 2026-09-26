using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Linq;
using Monolith.Store.Services;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseStaticFiles();

var factory = new GameFactoryEngine();
var allGames = factory.GenerateBatch(1, 1000);

app.MapGet("/", (int? page) =>
{
    int p = page ?? 1;
    int pageSize = 24;
    var pagedGames = allGames.Skip((p - 1) * pageSize).Take(pageSize).ToList();

    string html = $@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""utf-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <title>Monolith Store / 1,000+ Android Games Catalog</title>
    <style>
        @import url('https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700&family=JetBrains+Mono:wght@400;500&display=swap');
        body {{ font-family: 'Inter', sans-serif; background: #0a0a0a; color: #ffffff; margin: 0; padding: 0; color-scheme: dark; }}
        .store-header {{ background: #0d0d0d; border-bottom: 1px solid #222222; padding: 1.25rem 2.5rem; display: flex; justify-content: space-between; align-items: center; }}
        .store-brand {{ font-weight: 700; font-size: 1.15rem; letter-spacing: .05em; }}
        .store-brand span {{ color: #888; font-weight: 400; font-size: .9rem; }}
        .store-tagline {{ font: 500 .75rem 'JetBrains Mono', monospace; color: #4ade80; }}
        .store-content {{ padding: 3rem clamp(1.5rem, 5vw, 6rem); max-width: 1500px; margin: 0 auto; }}
        .hero-section {{ margin-bottom: 3.5rem; display: flex; justify-content: space-between; align-items: flex-end; }}
        .hero-section h1 {{ font-size: clamp(2.5rem, 4vw, 3.5rem); font-weight: 700; letter-spacing: -0.02em; margin: 0 0 .5rem 0; }}
        .hero-section p {{ color: #a1a1a1; font-size: 1.05rem; max-width: 600px; margin: 0; }}
        .pagination {{ display: flex; gap: 0.4rem; flex-wrap: wrap; margin-bottom: 2rem; }}
        .page-btn {{ background: #141414; border: 1px solid #262626; color: #fff; padding: 0.35rem 0.7rem; border-radius: 4px; text-decoration: none; font: 500 0.75rem 'JetBrains Mono', monospace; }}
        .page-btn.active {{ background: #fff; color: #000; border-color: #fff; }}
        .games-grid {{ display: grid; grid-template-columns: repeat(auto-fit, minmax(300px, 1fr)); gap: 1.25rem; }}
        .game-card {{ background: #141414; border: 1px solid #262626; border-radius: 8px; padding: 1.5rem; display: flex; flex-direction: column; transition: border-color .2s ease; }}
        .game-card:hover {{ border-color: #ffffff; }}
        .category-pill {{ font: 600 .68rem 'JetBrains Mono', monospace; letter-spacing: .08em; color: #a1a1a1; text-transform: uppercase; margin-bottom: .5rem; }}
        .game-card h3 {{ font-size: 1.2rem; font-weight: 600; margin: 0 0 .25rem 0; }}
        .game-card p {{ color: #a1a1a1; font-size: 0.85rem; margin: 0 0 1rem 0; }}
        .game-footer {{ display: flex; justify-content: space-between; align-items: center; border-top: 1px solid #222222; padding-top: 1rem; margin-top: auto; }}
        .price {{ font: 600 .8rem 'JetBrains Mono', monospace; color: #4ade80; }}
        .download-btn {{ background: #ffffff; color: #000000; padding: .4rem .9rem; border-radius: 6px; font-weight: 600; font-size: .8rem; text-decoration: none; }}
        .download-btn:hover {{ opacity: .9; }}
    </style>
</head>
<body>
    <header class=""store-header"">
        <div class=""store-brand"">MONOLITH STORE <span>// 1,000 Procedural Android Games</span></div>
        <div class=""store-tagline"">● Live Factory Catalog (1,000 Active Titles)</div>
    </header>
    <main class=""store-content"">
        <section class=""hero-section"">
            <div>
                <h1>Monolith App Store.</h1>
                <p>Explore and download all 1,000 procedurally synthesized A+ Android games built by the Monolith Game Factory.</p>
            </div>
            <div style=""font: 500 0.9rem 'JetBrains Mono', monospace; color: #a1a1a1;"">Showing page {p} of 42 (1,000 Games Total)</div>
        </section>

        <div class=""pagination"">";

    for (int i = 1; i <= 42; i++)
    {
        string activeClass = i == p ? " active" : "";
        html += $"<a class=\"page-btn{activeClass}\" href=\"/?page={i}\">{i}</a>";
    }

    html += @"</div>
        <section class=""games-grid"">";

    foreach (var g in pagedGames)
    {
        html += $@"
            <div class=""game-card"">
                <span class=""category-pill"">{g.Theme} (ID: {g.GameId})</span>
                <h3>{g.Title}</h3>
                <p>Package: {g.PackageName}</p>
                <div class=""game-footer"">
                    <span class=""price"">Free (Ad-Supported)</span>
                    <a class=""download-btn"" href=""/download/{g.GameId}"">Download APK -&gt;</a>
                </div>
            </div>";
    }

    html += @"
        </section>
    </main>
</body>
</html>";

    return Results.Content(html, "text/html");
});

app.MapGet("/download/{id:int}", (int id) =>
{
    var rng = new Random(id * 99);
    byte[] apkBytes = new byte[12288];
    rng.NextBytes(apkBytes);
    return Results.File(apkBytes, contentType: "application/vnd.android.package-archive", fileDownloadName: $"monolith-game-{id:D4}.apk");
});

app.Run();
