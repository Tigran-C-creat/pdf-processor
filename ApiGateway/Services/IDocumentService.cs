using Shared.Models;

namespace ApiGateway.Services;

/// <summary>
/// Сервис для работы с PDF‑документами.
/// Отвечает за загрузку файлов, сохранение метаданных,
/// публикацию сообщений в очередь и получение списка документов.
/// </summary>
public interface IDocumentService
{
    /// <summary>
    /// Загружает PDF‑файл, сохраняет его в файловом хранилище
    /// и инициирует асинхронную обработку через RabbitMQ.
    /// </summary>
    /// <param name="file">Загружаемый PDF‑файл.</param>
    /// <returns>Идентификатор созданного документа.</returns>
    Task<Guid> UploadAsync(IFormFile file);

    /// <summary>
    /// Возвращает список всех документов, отсортированных по дате создания.
    /// </summary>
    Task<List<Document>> GetAllAsync();
}