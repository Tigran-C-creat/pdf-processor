using Shared.Data;
using Shared.Models;
using ApiGateway.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiGateway.Controllers;

/// <summary>
/// Контроллер для работы с PDF‑документами.
/// </summary>

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IDocumentService _documentService;

    public DocumentsController(
        ApplicationDbContext db,
        IDocumentService documentService)
    {
        _db = db;
        _documentService = documentService;
    }

    /// <summary>
    /// Загружает PDF‑файл, сохраняет его на диск, создаёт запись в базе данных
    /// и отправляет сообщение в RabbitMQ для последующей обработки.
    /// </summary>
    /// <param name="file">Загружаемый PDF‑файл.</param>
    /// <returns>
    /// 200 — успешная загрузка, возвращает идентификатор документа.<br/>
    /// 400 — файл отсутствует или имеет неверный формат.
    /// </returns>

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Файл не загружен");

        if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Можно загружать только PDF");

        var id = await _documentService.UploadAsync(file);

        return Ok(new { id });
    }

    /// <summary>
    /// Возвращает список всех загруженных документов с их статусами обработки.
    /// </summary>
    /// <returns>Коллекция документов, отсортированная по дате создания.</returns>

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var docs = await _db.Documents
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

        return Ok(docs);
    }

    /// <summary>
    /// Возвращает текстовое содержимое PDF‑документа,
    /// которое было извлечено Background Worker'ом.
    /// </summary>
    /// <param name="id">Идентификатор документа.</param>
    /// <returns>
    /// 200 — текстовое содержимое документа.<br/>
    /// 202 — документ ещё обрабатывается.<br/>
    /// 404 — документ не найден.
    /// </returns>

    [HttpGet("{id:guid}/content")]
    public async Task<IActionResult> GetContent(Guid id)
    {
        var doc = await _db.Documents.FirstOrDefaultAsync(d => d.Id == id);

        if (doc == null)
            return NotFound();

        if (doc.Status == DocumentStatus.Pending || doc.Status == DocumentStatus.Processing)
            return StatusCode(202, "Документ ещё обрабатывается");

        if (doc.Status == DocumentStatus.Failed)
            return StatusCode(500, "Ошибка обработки документа");

        return Ok(new
        {
            id = doc.Id,
            text = doc.TextContent ?? string.Empty
        });
    }
}
