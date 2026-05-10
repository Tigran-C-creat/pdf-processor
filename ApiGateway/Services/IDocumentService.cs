namespace ApiGateway.Services;

/// <summary>
/// Контракт сервиса для работы с PDF‑документами.
/// Определяет операции по загрузке и обработке файлов.
/// </summary>
public interface IDocumentService
{
    /// <summary>
    /// Загружает PDF‑файл, сохраняет его в хранилище
    /// и инициирует асинхронную обработку через RabbitMQ.
    /// </summary>
    /// <param name="file">Загружаемый PDF‑файл.</param>
    /// <returns>Идентификатор созданного документа.</returns>
    Task<Guid> UploadAsync(IFormFile file);
}