using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NovelSeed.Data;
using NovelSeed.Models;

namespace NovelSeed.Controllers;

public class InspirationsController : Controller
{
    private readonly NovelSeedDbContext _db;

    public InspirationsController(NovelSeedDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(string? q, int page = 1, int pageSize = 20)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize switch
        {
            < 1 => 20,
            > 100 => 100,
            _ => pageSize
        };

        q = string.IsNullOrWhiteSpace(q) ? null : q.Trim();

        var query = _db.InspirationSeeds
            .AsNoTracking()
            .AsQueryable();

        if (q is not null)
        {
            var pattern = $"%{q}%";
            query = query.Where(x =>
                EF.Functions.Like(x.Topic, pattern) ||
                EF.Functions.Like(x.Name, pattern) ||
                EF.Functions.Like(x.Seed, pattern));
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        totalPages = totalPages < 1 ? 1 : totalPages;
        page = page > totalPages ? totalPages : page;

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var window = 2;
        var startPage = Math.Max(1, page - window);
        var endPage = Math.Min(totalPages, page + window);

        var vm = new InspirationsIndexVm
        {
            Items = items,
            Query = q,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            StartPage = startPage,
            EndPage = endPage
        };

        return View(vm);
    }

    public sealed class InspirationsIndexVm
    {
        public required IReadOnlyList<InspirationSeed> Items { get; init; }
        public string? Query { get; init; }
        public int Page { get; init; }
        public int PageSize { get; init; }
        public int TotalCount { get; init; }
        public int TotalPages { get; init; }
        public int StartPage { get; init; }
        public int EndPage { get; init; }
    }
}
