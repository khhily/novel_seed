namespace NovelSeed.Models;

public class WorkflowConfig
{
    public long Id { get; set; }

    public string AppName { get; set; } = "";

    public string ApiKey { get; set; } = "";

    public string? UserId { get; set; }

    public string? ApiHost { get; set; }
}

