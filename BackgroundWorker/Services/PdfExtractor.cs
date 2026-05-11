using System.Text;
using UglyToad.PdfPig;

namespace BackgroundWorker.Services;

/// <summary>
/// Извлекает текстовое содержимое из PDF-файла.
/// </summary>
public sealed class PdfExtractor : IPdfExtractor
{
    /// <summary>
    /// Извлекает текст из PDF-файла по указанному пути.
    /// </summary>
    public string ExtractText(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"PDF file not found: {filePath}");

        var sb = new StringBuilder();

        using var pdf = PdfDocument.Open(filePath);

        foreach (var page in pdf.GetPages())
            sb.AppendLine(page.Text);

        return sb.ToString();
    }
}
