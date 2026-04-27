using Microsoft.EntityFrameworkCore;
using NovelSeed.Models;

namespace NovelSeed.Data;

public class NovelSeedDbContext : DbContext
{
    public NovelSeedDbContext(DbContextOptions<NovelSeedDbContext> options) : base(options)
    {
    }

    public DbSet<InspirationSeed> InspirationSeeds => Set<InspirationSeed>();
    public DbSet<WorkflowConfig> WorkflowConfigs => Set<WorkflowConfig>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var inspiration = modelBuilder.Entity<InspirationSeed>();
        inspiration.ToTable("inspiration_seeds");
        inspiration.HasCharSet("utf8mb4");
        inspiration.UseCollation("utf8mb4_general_ci");
        inspiration.HasKey(x => x.Id);
        inspiration.Property(x => x.Topic).IsRequired().HasMaxLength(100).HasDefaultValue("");
        inspiration.Property(x => x.Name).IsRequired().HasMaxLength(255);
        inspiration.Property(x => x.Seed).IsRequired().HasColumnType("longtext");
        inspiration.Property(x => x.CreatedAt).IsRequired();

        var workflow = modelBuilder.Entity<WorkflowConfig>();
        workflow.ToTable("workflow_configs");
        workflow.HasCharSet("utf8mb4");
        workflow.UseCollation("utf8mb4_general_ci");
        workflow.HasKey(x => x.Id);
        workflow.Property(x => x.AppName).IsRequired().HasMaxLength(100);
        workflow.HasIndex(x => x.AppName).IsUnique();
        workflow.Property(x => x.ApiKey).IsRequired().HasMaxLength(255);
        workflow.HasIndex(x => x.ApiKey).IsUnique();
        workflow.Property(x => x.UserId).HasMaxLength(100);
        workflow.Property(x => x.ApiHost).HasMaxLength(255);
    }
}
