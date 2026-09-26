namespace Monolith.Tools;

public sealed class MediaEncoder
{
    public static void TranscodeAudioAsset(string inputPath, string outputPath)
    {
        Console.WriteLine($"[MEDIA ENCODER] Transcoding audio stem {inputPath} to optimized Opus/AAC stream at {outputPath}...");
    }

    public static void CompressTextureAtlas(string texturePath, int targetWidth, int targetHeight)
    {
        Console.WriteLine($"[MEDIA ENCODER] Compressing PBR texture {texturePath} to ASTC/ETC2 compressed GPU format ({targetWidth}x{targetHeight})...");
    }
}
