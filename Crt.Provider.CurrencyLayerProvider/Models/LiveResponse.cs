namespace Crt.Provider.CurrencyLayerProvider.Models;

/// <summary>
/// Ответ - текущий курс.
/// </summary>
internal class LiveResponse : BaseQuoteResponse
{
    /// <summary>
    /// Курсы валют.
    /// </summary>
    public Dictionary<string, decimal> Quotes { get; set; }

    /// <summary>
    /// Дата.
    /// </summary>
    public int TimeStamp { get; set; }
}