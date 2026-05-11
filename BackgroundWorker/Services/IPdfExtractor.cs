namespace BackgroundWorker.Services;

/// <summary>
/// Извлекает текст из PDF-файла.
/// </summary>
public interface IPdfExtractor
{
    /// <summary>
    /// Извлекает текст из PDF по указанному пути.
    /// </summary>
    string ExtractText(string filePath);
}
