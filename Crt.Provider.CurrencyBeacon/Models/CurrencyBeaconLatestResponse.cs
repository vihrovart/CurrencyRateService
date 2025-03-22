namespace Crt.Provider.CurrencyBeacon.Models;

using Newtonsoft.Json;

/// <summary>
/// Модель ответа на запрос "latest" (текущие курсы).
/// </summary>
public class CurrencyBeaconLatestResponse
{
    /// <summary>
    /// Показывает, успешно ли выполнен запрос.
    /// </summary>
    [JsonProperty("success")]
    public bool Success { get; set; }

    /// <summary>
    /// Базовая валюта.
    /// </summary>
    [JsonProperty("base")]
    public string BaseCurrency { get; set; }

    /// <summary>
    /// Дата, на которую актуальны курсы (формат YYYY-MM-DD).
    /// </summary>
    [JsonProperty("date")]
    public string Date { get; set; }

    /// <summary>
    /// Словарь Валюта->Курс.
    /// </summary>
    [JsonProperty("rates")]
    public Dictionary<string, decimal> Rates { get; set; }

    /// <summary>
    /// Информация об ошибке (если есть).
    /// </summary>
    [JsonProperty("error", NullValueHandling = NullValueHandling.Ignore)]
    public CurrencyBeaconError? Error { get; set; }
}