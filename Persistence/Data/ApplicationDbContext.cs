using Microsoft.EntityFrameworkCore;
using Persistence.Models;


namespace Persistence.Data;

/// <summary>
/// Контекст базы данных приложения.
/// </summary>
public sealed class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Таблица документов.
    /// </summary>
    public DbSet<Document> Documents => Set<Document>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Document>().ToTable("documents");
    }
}
