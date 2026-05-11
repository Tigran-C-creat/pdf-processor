namespace Persistence.Models;

public class Document
{
    public Guid Id { get; set; }

    public string FileName { get; set; } = default!;

    public string OriginalName { get; set; } = default!;

    public string FilePath { get; set; } = default!;

    public DocumentStatus Status { get; set; }

    public string? TextContent { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

}
