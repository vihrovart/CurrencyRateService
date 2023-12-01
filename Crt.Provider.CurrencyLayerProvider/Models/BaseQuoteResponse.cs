namespace Crt.Provider.CurrencyLayerProvider.Models;

/// <summary>
/// Базовый ответ с курсами валют.
/// </summary>
internal class BaseQuoteResponse : BaseResponse
{
    /// <summary>
    /// Исходная валюта.
    /// </summary>
    public string Source { get; set; }
}