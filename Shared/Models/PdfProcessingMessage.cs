namespace Shared.Models;

public sealed class PdfProcessingMessage
{
    public Guid DocumentId { get; set; }
    public string FilePath { get; set; } = default!;
}
