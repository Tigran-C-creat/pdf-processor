using Persistence.Models;

namespace BackgroundWorker.Services;

public interface IDocumentProcessingService
{
    Task<Document?> GetAsync(Guid id, CancellationToken ct);
    Task SetStatusAsync(Document doc, DocumentStatus status, CancellationToken ct);
    Task SaveExtractedTextAsync(Document doc, string text, CancellationToken ct);
}
