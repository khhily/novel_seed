using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using NovelSeed.Data;
using NovelSeed.Models;

namespace NovelSeed.Controllers;

[ApiController]
[Route("api/ingest")]
public class InspirationIngestController : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly NovelSeedDbContext _db;

    public InspirationIngestController(NovelSeedDbContext db)
    {
        _db = db;
    }

    public sealed class IngestRequest
    {
        public List<InspirationItemDto>? Output { get; set; }
    }

    public sealed class InspirationItemDto
    {
        public string? Topic { get; set; }
        public string? Name { get; set; }
        public string? Seed { get; set; }
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] IngestRequest request)
    {
        try
        {
            var items = request.Output;
            // JsonSerializer.Deserialize<List<InspirationItemDto>>(request.Output, JsonOptions);
            if (items is null || items.Count == 0)
            {
                return Ok();
            }

            var entities = new List<InspirationSeed>(items.Count);
            foreach (var item in items)
            {
                if (string.IsNullOrWhiteSpace(item.Name) || string.IsNullOrWhiteSpace(item.Seed))
                {
                    continue;
                }

                entities.Add(new InspirationSeed
                {
                    Topic = (item.Topic ?? "").Trim(),
                    Name = item.Name.Trim(),
                    Seed = item.Seed.Trim(),
                    CreatedAt = DateTimeOffset.UtcNow
                });
            }

            if (entities.Count == 0)
            {
                return Ok();
            }

            await _db.InspirationSeeds.AddRangeAsync(entities);
            await _db.SaveChangesAsync();
        }
        catch
        {
            return Ok();
        }

        return Ok();
    }
}
