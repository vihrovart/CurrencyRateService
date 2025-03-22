namespace Crt.Provider.CurrencyBeacon.Models;

using Newtonsoft.Json;

/// <summary>
/// Модель ответа на запрос "timeframe".
/// </summary>
public class CurrencyBeaconTimeFrameResponse
{
    /// <summary>
    /// Показывает, успешно ли выполнен запрос.
    /// </summary>
    [JsonProperty("success")]
    public bool Success { get; set; }

    /// <summary>
    /// Код базовой валюты (например, "EUR").
    /// </summary>
    [JsonProperty("base")]
    public string BaseCurrency { get; set; }

    /// <summary>
    /// Дата начала интервала (формат YYYY-MM-DD).
    /// </summary>
    [JsonProperty("start_date")]
    public string StartDate { get; set; }

    /// <summary>
    /// Дата окончания интервала (формат YYYY-MM-DD).
    /// </summary>
    [JsonProperty("end_date")]
    public string EndDate { get; set; }

    /// <summary>
    /// Словарь дат -> (словарь Валюта->Курс).
    /// </summary>
    [JsonProperty("rates")]
    public Dictionary<string, Dictionary<string, decimal>> Rates { get; set; }

    /// <summary>
    /// Информация об ошибке (опционально, если success=false).
    /// </summary>
    [JsonProperty("error", NullValueHandling = NullValueHandling.Ignore)]
    public CurrencyBeaconError Error { get; set; }
}