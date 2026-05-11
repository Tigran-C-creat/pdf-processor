namespace Persistence.Models;

/// <summary>
/// Статус обработки PDF‑документа.
/// </summary>
public enum DocumentStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3
}
