namespace Monolith.Shared;

using System;
using System.Text.Json;

public sealed class EnterpriseLogEntry
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Level { get; set; } = "INFO";
    public string Component { get; set; } = "MonolithCore";
    public string Message { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
}

public sealed class EnterpriseLogger
{
    public static void LogStructured(string level, string component, string message)
    {
        var entry = new EnterpriseLogEntry
        {
            Level = level,
            Component = component,
            Message = message
        };
        string json = JsonSerializer.Serialize(entry);
        Console.WriteLine($"[MONOLITH ENTERPRISE LOG] {json}");
    }
}
