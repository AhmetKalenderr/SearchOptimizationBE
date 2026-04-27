using Microsoft.EntityFrameworkCore;
using SearchOptimizationBE.Models;

namespace SearchOptimizationBE.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentType> DocumentTypes => Set<DocumentType>();
    public DbSet<User> Users => Set<User>();
    public DbSet<DocumentToken> DocumentTokens => Set<DocumentToken>();
    public DbSet<DocumentContentHash> DocumentContentHashes => Set<DocumentContentHash>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
