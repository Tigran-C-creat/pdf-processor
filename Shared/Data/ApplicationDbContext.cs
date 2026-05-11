using Microsoft.EntityFrameworkCore;
using Shared.Models;


namespace Shared.Data;

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
}
