namespace Crt.Provider.Frankfurter.Models;

using Newtonsoft.Json;

/// <summary>
/// Представляет ответ сервиса Frankfurter на запрос последних (актуальных) курсов.
/// </summary>
public class FrankfurterLatestResponse
{
    /// <summary>
    /// Словарь ставок (курсов), где ключ — это код валюты (например, "USD"),
    /// а значение — курс к базовой валюте.
    /// </summary>
    [JsonProperty("rates")]
    public Dictionary<string, decimal> Rates { get; set; }

    /// <summary>
    /// Получает или задаёт базовую (исходную) валюту, указанную в запросе (параметр "from").
    /// </summary>
    [JsonProperty("base")]
    public string BaseCurrency { get; set; }

    /// <summary>
    /// Дата, на которую актуальны возвращённые курсы (в формате "yyyy-MM-dd").
    /// </summary>
    [JsonProperty("date")]
    public string Date { get; set; }
}