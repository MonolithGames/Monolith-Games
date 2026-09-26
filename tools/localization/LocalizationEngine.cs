namespace Monolith.Tools;

public sealed class LocalizationEngine
{
    public static void TranslateStoreListing(string text, string targetLanguage)
    {
        Console.WriteLine($"[LOCALIZATION] Translating asset to [{targetLanguage}]: {text}");
    }
}
