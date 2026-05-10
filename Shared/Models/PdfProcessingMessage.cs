namespace Shared.Models;

/// <summary>
/// Сообщение, отправляемое в RabbitMQ для обработки PDF‑файла.
/// </summary>
public sealed class PdfProcessingMessage
{
    public Guid DocumentId { get; set; }
    public string FilePath { get; set; } = default!;
}
