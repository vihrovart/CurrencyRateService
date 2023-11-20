namespace Crt.Provider.CurrencyLayerProvider.Models;

using Newtonsoft.Json;

/// <summary>
/// Базовая модель.
/// </summary>
internal class BaseResponse
{
    /// <summary>
    /// Признак удачного выполнения.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Ошибка.
    /// </summary>
    public ErrorModel Error { get; set; }
}