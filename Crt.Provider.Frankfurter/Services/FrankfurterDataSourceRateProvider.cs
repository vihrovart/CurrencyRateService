namespace Crt.Provider.Frankfurter.Services;

using System.Globalization;
using Crt.Core.Exceptions;
using Crt.Core.Models;
using Crt.Core.Services;
using Crt.Provider.Frankfurter.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

/// <summary>
/// Провайдер курсов валют на основе источника Frankfurter (https://www.frankfurter.app/).
/// Реализует интерфейс <see cref="IDataSourceRateProvider"/>.
/// </summary>
public class FrankfurterDataSourceRateProvider : IDataSourceRateProvider
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly ILogger<FrankfurterDataSourceRateProvider> logger;
    private readonly ProviderSettings settings;

    private readonly object locker = new object();
    private DateTime lastRequestTime;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="FrankfurterDataSourceRateProvider"/>.
    /// </summary>
    /// <param name="httpClientFactory">Фабрика для создания HTTP-клиентов.</param>
    /// <param name="settingsProvider">Провайдер настроек для данного источника.</param>
    /// <param name="logger">Экземпляр логгера.</param>
    /// <exception cref="Exception">Выбрасывается, если не найдены настройки для данного провайдера.</exception>
    public FrankfurterDataSourceRateProvider(
        IHttpClientFactory httpClientFactory,
        ISettingsProvider settingsProvider,
        ILogger<FrankfurterDataSourceRateProvider> logger)
    {
        this.lastRequestTime = DateTime.Now;
        this.httpClientFactory = httpClientFactory;
        this.logger = logger;

        // Ищем настройки по имени провайдера
        this.settings = settingsProvider.TryGetSettings(this.DataSourceName)
            ?? throw new CrtException("Не найдены настройки для FrankfurterDataSourceRateProvider");
    }

    /// <summary>
    /// Получает название источника данных.
    /// </summary>
    public string DataSourceName => "Frankfurter";

    /// <inheritdoc/>
    public string DataSourceNodeName => this.settings.NodeName;

    /// <summary>
    /// Получает максимально допустимую разницу между датами промежутка.
    /// </summary>
    public int MaxDateDifference => this.settings.MaxDateDifference;

    /// <inheritdoc/>
    public int DayRequestCount => this.settings.DayRequestCount;

    /// <inheritdoc/>
    public int MonthRequestCount => this.settings.MonthRequestCount;

    /// <summary>
    /// Получает исторические значения курсов валют за указанный промежуток дат относительно
    /// указанной валюты.
    /// </summary>
    /// <param name="startDate">Дата начала (включительно).</param>
    /// <param name="endDate">Дата окончания (включительно).</param>
    /// <param name="currency">Код базовой (исходной) валюты (например, "EUR").</param>
    /// <returns>Массив значений курсов за указанный диапазон.</returns>
    public async Task<RateValue[]> GetTimeFrameRates(DateTime startDate, DateTime endDate, string currency)
    {
        // Формат даты "yyyy-MM-dd" - это ожидаемый формат у Frankfurter
        var startDateParameter = startDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var endDateParameter = endDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        var url = $"https://api.frankfurter.app/{startDateParameter}..{endDateParameter}?from={currency}";

        var response = await ExecuteProviderRequest<FrankfurterTimeSeriesResponse>(url);

        var data = response
            .Rates
            .SelectMany(dateRatesPair =>
            {
                var parsedDate = DateTime.Parse(dateRatesPair.Key, CultureInfo.InvariantCulture);
                return dateRatesPair.Value.Select(currencyValuePair =>
                    new RateValue
                    {
                        DataSource = this.DataSourceName,
                        SourceCurrency = response.BaseCurrency,
                        Date = parsedDate,
                        Currency = currencyValuePair.Key,
                        CurrencyPairValue = $"{response.BaseCurrency}{currencyValuePair.Key}",
                        Value = currencyValuePair.Value
                    });
            })
            .ToArray();

        return data;
    }

    /// <summary>
    /// Получает текущие (актуальные) значения курсов валют относительно указанной базовой валюты.
    /// </summary>
    /// <param name="currency">Код базовой (исходной) валюты (например, "EUR").</param>
    /// <returns>Массив значений курсов на актуальную дату.</returns>
    public async Task<RateValue[]> GetCurrentValue(string currency)
    {
        var url = $"https://api.frankfurter.app/latest?from={currency}";

        var response = await ExecuteProviderRequest<FrankfurterLatestResponse>(url);

        var parsedDate = DateTime.Parse(response.Date, CultureInfo.InvariantCulture);

        var data = response
            .Rates
            .Select(pair =>
                new RateValue
                {
                    DataSource = this.DataSourceName,
                    SourceCurrency = response.BaseCurrency,
                    Date = parsedDate,
                    Currency = pair.Key,
                    CurrencyPairValue = $"{response.BaseCurrency}{pair.Key}",
                    Value = pair.Value
                })
            .ToArray();

        return data;
    }

    /// <summary>
    /// Выполняет запрос к сервису Frankfurter и десериализует результат в объект типа <typeparamref name="T"/>.
    /// Реализует логику задержки (не чаще чем раз в 100 мс).
    /// </summary>
    /// <typeparam name="T">Тип результата (модель ответа).</typeparam>
    /// <param name="url">URL для обращения к API Frankfurter.</param>
    /// <returns>Объект, десериализованный из ответа сервиса.</returns>
    /// <exception cref="Exception">
    /// Выбрасывается, если ответ сервера неуспешен (статус-код не 200)
    /// или не удалось разобрать (десериализовать) ответ.
    /// </exception>
    private async Task<T> ExecuteProviderRequest<T>(string url)
    {
        using var httpClient = this.httpClientFactory.CreateClient();

        HttpResponseMessage response;
        string responseContent;

        lock (this.locker)
        {
            // Минимальная задержка 100 мс, чтобы не отправлять запросы слишком часто
            while (DateTime.Now.Subtract(this.lastRequestTime).Milliseconds < 100)
            {
            }

            response = httpClient.GetAsync(new Uri(url)).ConfigureAwait(false).GetAwaiter().GetResult();
            this.lastRequestTime = DateTime.Now;
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new CrtException($"Ошибка при запросе к сервису Frankfurter ({response.StatusCode})");
        }

        responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        var responseResult = JsonConvert.DeserializeObject<T>(responseContent);

        if (responseResult == null)
        {
            throw new CrtException("Не удалось разобрать ответ сервиса Frankfurter");
        }

        return responseResult;
    }
}