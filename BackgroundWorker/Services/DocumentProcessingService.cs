using Microsoft.EntityFrameworkCore;
using Persistence.Data;
using Persistence.Models;

namespace BackgroundWorker.Services;

/// <summary>
/// Сервис для работы с документами в базе данных.
/// </summary>
public class DocumentProcessingService(ApplicationDbContext db) : IDocumentProcessingService
{
    /// <summary>
    /// Возвращает документ по идентификатору.
    /// </summary>
    public async Task<Document?> GetAsync(Guid id, CancellationToken ct)
    {
        return await db.Documents.FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    /// <summary>
    /// Обновляет статус документа.
    /// </summary>
    public async Task SetStatusAsync(Document doc, DocumentStatus status, CancellationToken ct)
    {
        doc.Status = status;
        await db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Сохраняет извлечённый текст и обновляет статус документа на Completed.
    /// </summary>
    public async Task SaveExtractedTextAsync(Document doc, string text, CancellationToken ct)
    {
        doc.TextContent = text;
        doc.Status = DocumentStatus.Completed;
        doc.ProcessedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
    }
}
