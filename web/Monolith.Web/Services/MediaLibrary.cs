namespace Monolith.Web.Services;

public sealed class MediaLibrary(IWebHostEnvironment environment)
{
    private readonly string mediaPath = Path.Combine(environment.ContentRootPath, "App_Data", "Media");

    public IReadOnlyList<string> GetFiles()
    {
        Directory.CreateDirectory(mediaPath);
        return Directory.EnumerateFiles(mediaPath)
            .Select(Path.GetFileName)
            .Where(name => name is not null)
            .Cast<string>()
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public async Task<string> SaveAsync(IFormFile file, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(mediaPath);
        var safeName = Path.GetFileName(file.FileName);
        if (string.IsNullOrWhiteSpace(safeName))
            throw new InvalidOperationException("A file name is required.");

        var destination = Path.Combine(mediaPath, $"{DateTime.UtcNow:yyyyMMddHHmmssfff}-{safeName}");
        await using var output = File.Create(destination);
        await file.CopyToAsync(output, cancellationToken);
        return Path.GetFileName(destination);
    }
}
