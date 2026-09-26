namespace Monolith.Data;

public sealed class DataOptions
{
    public int MaximumRecords { get; set; } = 10_000;
    public int MaximumKeyLength { get; set; } = 128;
    public int MaximumValueBytes { get; set; } = 64 * 1024;
    public int MaximumPageSize { get; set; } = 100;
}