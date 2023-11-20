namespace Crt.Provider.CurrencyLayerProvider.Models;

using Newtonsoft.Json;

/// <summary>
/// Ответ - временной отрезок.
/// </summary>
internal class TimeFrameResponse : BaseQuoteResponse
{
    /// <summary>
    /// Признак того, что это временной отрезок.
    /// </summary>
    public bool TimeFrame { get; set; }

    /// <summary>
    /// Дата начала.
    /// </summary>
    [JsonProperty("start_date")]
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Дата окончания.
    /// </summary>
    [JsonProperty("end_date")]
    public DateTime EndDate { get; set; }
}