namespace Crt.Provider.CurrencyLayerProvider.Models;

/// <summary>
/// Модель ошибки.
/// </summary>
internal class ErrorModel
{
    /// <summary>
    /// Код ошибки.
    /// </summary>
    public int Code { get; set; }

    /// <summary>
    /// Информация об ошибке.
    /// </summary>
    public string Info { get; set; }
}