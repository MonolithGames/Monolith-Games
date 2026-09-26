namespace Monolith.Services;

public sealed class MediaLibrary(IWebHostEnvironment environment)
{
    private const long MaxFileSize = 250 * 1024 * 1024;
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        { ".png", ".jpg", ".jpeg", ".gif", ".webp", ".wav", ".mp3", ".mp4", ".obj", ".fbx" };
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
        if (file.Length == 0 || file.Length > MaxFileSize)
            throw new InvalidOperationException("Media files must be between 1 byte and 250 MB.");
        if (!AllowedExtensions.Contains(Path.GetExtension(safeName)))
            throw new InvalidOperationException("That media file type is not supported.");

        var destination = Path.Combine(mediaPath, $"{DateTime.UtcNow:yyyyMMddHHmmssfff}-{safeName}");
        await using var output = File.Create(destination);
        await file.CopyToAsync(output, cancellationToken);
        return Path.GetFileName(destination);
    }
}
