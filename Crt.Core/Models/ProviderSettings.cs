namespace Crt.Core.Models;

/// <summary>
/// Настройки провайдера.
/// </summary>
public class ProviderSettings
{
    /// <summary>
    /// Название провайдера.
    /// </summary>
    public string ProviderName { get; set; }

    /// <summary>
    /// Допустимое количество запросов в сутки.
    /// </summary>
    public int DayRequestCount { get; set; }

    /// <summary>
    /// Максимальное количество дней между датами в запросе.
    /// </summary>
    public int MaxDateDifference { get; set; }

    /// <summary>
    /// Ключ.
    /// </summary>
    public string Key { get; set; }
}