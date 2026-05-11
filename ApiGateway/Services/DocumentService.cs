using Persistence.Data;
using RabbitMQ.Client;
using Persistence.Models;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Contracts.Messages;


namespace ApiGateway.Services;

/// <summary>
/// Сервис для загрузки PDF‑документов, сохранения их в файловом хранилище,
/// создания записей в базе данных и публикации сообщений в RabbitMQ.
/// </summary>
public sealed class DocumentService(
        ApplicationDbContext db,
        IConnection rabbit,
        IWebHostEnvironment env) : IDocumentService
{
    private const string QueueName = "pdf_processing";

    /// <summary>
    /// Загружает PDF‑файл, сохраняет его в файловом хранилище
    /// и инициирует асинхронную обработку через RabbitMQ.
    /// </summary>
    /// <param name="file">Загружаемый PDF‑файл.</param>
    /// <returns>Идентификатор созданного документа.</returns>
    public async Task<Guid> UploadAsync(IFormFile file)
    {
        var storagePath = Path.Combine(env.ContentRootPath, "storage");
        Directory.CreateDirectory(storagePath);

        var id = Guid.NewGuid();
        var storedFileName = $"{id}.pdf";
        var filePath = Path.Combine(storagePath, storedFileName);

        using (var stream = System.IO.File.Create(filePath))
            await file.CopyToAsync(stream);

        var doc = new Document
        {
            Id = id,
            FileName = storedFileName,
            OriginalName = file.FileName,
            FilePath = filePath,
            Status = DocumentStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            ProcessedAt = null
        };

        db.Documents.Add(doc);
        await db.SaveChangesAsync();

        await using var channel = await rabbit.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false
        );

        var message = new PdfProcessingMessage
        {
            DocumentId = id,
            FilePath = filePath
        };

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: QueueName,
            mandatory: false,
            body: body
        );

        return id;
    }

    /// <summary>
    /// Возвращает список всех документов, отсортированных по дате создания.
    /// </summary>
    public async Task<List<Document>> GetAllAsync()
    {
        return await db.Documents
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Возвращает текстовое содержимое PDF‑документа
    /// </summary>
    /// <param name="id">Идентификатор документа.</param>
    public async Task<(int StatusCode, string? Content)> GetContentAsync(Guid id)
    {
        var doc = await db.Documents.FirstOrDefaultAsync(d => d.Id == id);

        if (doc == null)
            return (404, null);

        if (doc.Status == DocumentStatus.Pending || doc.Status == DocumentStatus.Processing)
            return (202, "Документ ещё обрабатывается");

        if (doc.Status == DocumentStatus.Failed)
            return (500, "Ошибка обработки документа");

        return (200, doc.TextContent ?? string.Empty);
    }
}
