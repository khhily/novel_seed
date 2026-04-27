using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NovelSeed.Data;
using NovelSeed.Models;

namespace NovelSeed.Controllers;

public class WorkflowConfigsController : Controller
{
    private readonly NovelSeedDbContext _db;

    public WorkflowConfigsController(NovelSeedDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = 20)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize switch
        {
            < 1 => 20,
            > 100 => 100,
            _ => pageSize
        };

        var query = _db.WorkflowConfigs.AsNoTracking();

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        totalPages = totalPages < 1 ? 1 : totalPages;
        page = page > totalPages ? totalPages : page;

        var items = await query
            .OrderBy(x => x.AppName)
            .ThenBy(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var window = 2;
        var startPage = Math.Max(1, page - window);
        var endPage = Math.Min(totalPages, page + window);

        var vm = new WorkflowConfigsIndexVm
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            StartPage = startPage,
            EndPage = endPage
        };

        return View(vm);
    }

    public sealed class WorkflowConfigsIndexVm
    {
        public required IReadOnlyList<WorkflowConfig> Items { get; init; }
        public int Page { get; init; }
        public int PageSize { get; init; }
        public int TotalCount { get; init; }
        public int TotalPages { get; init; }
        public int StartPage { get; init; }
        public int EndPage { get; init; }
    }
}

