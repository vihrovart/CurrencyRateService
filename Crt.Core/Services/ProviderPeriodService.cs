namespace Crt.Core.Services;

using Newtonsoft.Json;

/// <summary>
/// Сервис для отслеживания количества запросов к провайдерам и результата последнего запроса.
/// </summary>
public class ProviderPeriodService
{
    private const string DataFilePath = "provider_stats.json"; // TODO: заменить на настройку при необходимости

    private readonly Dictionary<string, ProviderStats> providers = new ();

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="ProviderPeriodService"/> и загружает данные с диска.
    /// </summary>
    public ProviderPeriodService()
    {
        if (!File.Exists(DataFilePath))
        {
            return;
        }

        var json = File.ReadAllText(DataFilePath);
        var loaded = JsonConvert.DeserializeObject<Dictionary<string, ProviderStats>>(json);
        if (loaded != null)
        {
            this.providers = loaded;
        }
    }

    /// <summary>
    /// Увеличивает количество запросов для указанного провайдера.
    /// </summary>
    /// <param name="providerName">Название провайдера.</param>
    public void IncrementRequestCount(string providerName)
    {
        var today = DateTime.Today;

        if (!this.providers.TryGetValue(providerName, out var stats))
        {
            stats = new ProviderStats { LastRequestDate = today };
            this.providers[providerName] = stats;
        }

        if (stats.LastRequestDate.Date != today)
        {
            stats.DayCount = 0;
            if (stats.LastRequestDate.Month != today.Month || stats.LastRequestDate.Year != today.Year)
            {
                stats.MonthCount = 0;
            }

            stats.LastRequestDate = today;
        }

        stats.DayCount++;
        stats.MonthCount++;

        this.Save();
    }

    /// <summary>
    /// Устанавливает статус последнего запроса для провайдера.
    /// </summary>
    /// <param name="providerName">Название провайдера.</param>
    /// <param name="wasSuccessful">True, если последний запрос был удачным, иначе false.</param>
    public void SetLastSuccess(string providerName, bool wasSuccessful)
    {
        if (!this.providers.TryGetValue(providerName, out var stats))
        {
            stats = new ProviderStats { LastRequestDate = DateTime.Today };
            this.providers[providerName] = stats;
        }

        stats.WasLastSuccessful = wasSuccessful;

        this.Save();
    }

    /// <summary>
    /// Получает информацию по провайдеру: количество запросов и статус последнего запроса.
    /// </summary>
    /// <param name="providerName">Название провайдера.</param>
    /// <returns>Информация по провайдеру.</returns>
    public (int DayCount, int MonthCount, bool WasLastSuccessful) GetStats(string providerName)
    {
        if (!this.providers.TryGetValue(providerName, out var stats))
        {
            return (0, 0, true);
        }

        var today = DateTime.Today;
        if (stats.LastRequestDate.Date != today)
        {
            stats.DayCount = 0;
            if (stats.LastRequestDate.Month != today.Month || stats.LastRequestDate.Year != today.Year)
            {
                stats.MonthCount = 0;
            }

            stats.LastRequestDate = today;
            this.Save();
        }

        return (stats.DayCount, stats.MonthCount, stats.WasLastSuccessful);
    }

    private void Save()
    {
        var json = JsonConvert.SerializeObject(this.providers, Formatting.Indented);
        File.WriteAllText(DataFilePath, json);
    }

    private class ProviderStats
    {
        public int DayCount { get; set; }

        public int MonthCount { get; set; }

        public bool WasLastSuccessful { get; set; } = true;

        public DateTime LastRequestDate { get; set; }
    }
}