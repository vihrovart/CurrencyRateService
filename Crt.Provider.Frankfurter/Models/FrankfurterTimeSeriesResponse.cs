namespace Crt.Provider.Frankfurter.Models;

using Newtonsoft.Json;

/// <summary>
/// Представляет ответ сервиса Frankfurter на запрос временного диапазона (timeframe).
/// </summary>
public class FrankfurterTimeSeriesResponse
{
    /// <summary>
    /// Словарь ставок (курсов), где ключ — это дата в формате "yyyy-MM-dd",
    /// а значение — это словарь, в котором ключ — код валюты (например, "USD"),
    /// а значение — курс к базовой валюте.
    /// </summary>
    [JsonProperty("rates")]
    public Dictionary<string, Dictionary<string, decimal>> Rates { get; set; }

    /// <summary>
    /// Получает или задаёт базовую (исходную) валюту, указанную в запросе (параметр "from").
    /// </summary>
    [JsonProperty("base")]
    public string BaseCurrency { get; set; }

    /// <summary>
    /// Дата начала диапазона (в формате "yyyy-MM-dd"), которую вернул сервис.
    /// </summary>
    [JsonProperty("start_date")]
    public string StartDate { get; set; }

    /// <summary>
    /// Дата окончания диапазона (в формате "yyyy-MM-dd"), которую вернул сервис.
    /// </summary>
    [JsonProperty("end_date")]
    public string EndDate { get; set; }
}