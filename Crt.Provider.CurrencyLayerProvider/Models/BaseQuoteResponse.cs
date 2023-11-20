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

    /// <summary>
    /// Курсы валют.
    /// </summary>
    public Dictionary<DateTime, Dictionary<string, decimal>> Quotes { get; set; }
}