using Shared.Models;

namespace ApiGateway.Models;

/// <summary>
/// Модель сущности документа, хранящегося в базе данных.
/// </summary>
/// <remarks>
/// Содержит:
/// - Имя файла в хранилище.
/// - Оригинальное имя файла.
/// - Путь к файлу.
/// - Статус обработки.
/// - Извлечённый текст.
/// - Даты создания и обновления.
/// </remarks>
public sealed class Document
{
    public Guid Id { get; set; }

    public string FileName { get; set; } = default!;

    public string OriginalName { get; set; } = default!;

    public string FilePath { get; set; } = default!;

    public DocumentStatus Status { get; set; }

    public string? TextContent { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
