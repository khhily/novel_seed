namespace NovelSeed.Models;

public class InspirationSeed
{
    public long Id { get; set; }

    public string Topic { get; set; } = "";

    public string Name { get; set; } = "";

    public string Seed { get; set; } = "";

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

