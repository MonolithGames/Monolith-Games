var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Monolith Telemetry Service Running");

app.Run();
