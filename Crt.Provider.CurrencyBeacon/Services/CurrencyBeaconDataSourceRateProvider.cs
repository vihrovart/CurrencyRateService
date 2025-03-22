namespace Crt.Provider.CurrencyBeacon.Services;

using System.Globalization;
using Crt.Core.Exceptions;
using Crt.Core.Models;
using Crt.Core.Services;
using Crt.Provider.CurrencyBeacon.Constant;
using Crt.Provider.CurrencyBeacon.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

/// <summary>
/// Провайдер курсов валют - CurrencyBeacon.
/// </summary>
public class CurrencyBeaconDataSourceRateProvider : IDataSourceRateProvider
{
    private readonly ProviderPeriodService providerPeriodService;
    private readonly IHttpClientFactory httpClientFactory;
    private readonly ILogger<CurrencyBeaconDataSourceRateProvider> logger;
    private readonly ProviderSettings settings;
    private readonly string apiKey;

    private readonly object locker = new object();
    private DateTime lastRequestTime;

    /// <summary>
    /// Initializes a new instance of the <see cref="CurrencyBeaconDataSourceRateProvider"/> class.
    /// </summary>
    /// <param name="providerPeriodService">Сервис для отслеживания количества запросов.</param>
    /// <param name="httpClientFactory">Фабрика http клиента.</param>
    /// <param name="settingsProvider">Поставщик настроек.</param>
    /// <param name="logger">Логгер.</param>
    public CurrencyBeaconDataSourceRateProvider(
        ProviderPeriodService providerPeriodService,
        IHttpClientFactory httpClientFactory,
        ISettingsProvider settingsProvider,
        ILogger<CurrencyBeaconDataSourceRateProvider> logger)
    {
        this.lastRequestTime = DateTime.Now;
        this.providerPeriodService = providerPeriodService;
        this.httpClientFactory = httpClientFactory;
        this.logger = logger;

        this.settings = settingsProvider.TryGetSettings(this.DataSourceName)
            ?? throw new CrtException(ProviderConstant.Exceptions.ExceptionNoSettingsForProvider);

        if (!File.Exists(this.settings.KeyFile))
        {
            throw new CrtException(ProviderConstant.Exceptions.ExceptionKeyFileNotFound);
        }

        this.apiKey = File.ReadAllText(this.settings.KeyFile);
    }

    /// <inheritdoc/>
    public string DataSourceName => "CurrencyBeacon";

    /// <inheritdoc/>
    public string DataSourceNodeName => this.settings.NodeName;

    /// <inheritdoc/>
    public int MaxDateDifference => this.settings.MaxDateDifference;

    /// <inheritdoc/>
    public int DayRequestCount => this.settings.DayRequestCount;

    /// <inheritdoc/>
    public int MonthRequestCount => this.settings.MonthRequestCount;

    /// <inheritdoc/>
    public Task<RateValue[]> GetTimeFrameRates(DateTime startDate, DateTime endDate, string currency)
    {
        var startDateParameter = startDate.ToString(ProviderConstant.RequestDateFormat, CultureInfo.CurrentCulture);
        var endDateParameter = endDate.ToString(ProviderConstant.RequestDateFormat, CultureInfo.CurrentCulture);

        var url = string.Format(
            CultureInfo.CurrentCulture,
#pragma warning disable CA1863
            ProviderConstant.Urls.TimeFrame,
#pragma warning restore CA1863
            this.apiKey,
            currency,
            startDateParameter,
            endDateParameter);

        return this.ExecuteTimeFrameRatesRequest(url);
    }

    /// <inheritdoc/>
    public Task<RateValue[]> GetCurrentValue(string currency)
    {
        var url = string.Format(
            CultureInfo.CurrentCulture,
#pragma warning disable CA1863
            ProviderConstant.Urls.Latest,
#pragma warning restore CA1863
            this.apiKey,
            currency);

        return this.ExecuteLatestRatesRequest(url);
    }

    private async Task<RateValue[]> ExecuteTimeFrameRatesRequest(string url)
    {
        var responseResult = await this.ExecuteProviderRequest<CurrencyBeaconTimeFrameResponse>(url);

        if (responseResult.Error != null)
        {
            var message = responseResult.Error != null
#pragma warning disable CA1863
                ? string.Format(CultureInfo.CurrentCulture, ProviderConstant.Exceptions.ExceptionRequestError, responseResult.Error.Code, responseResult.Error.Message)
#pragma warning restore CA1863
                : ProviderConstant.Exceptions.ExceptionRequestUnSupportedError;
            throw new CrtException(message);
        }

        var result = new List<RateValue>();

        foreach (var dateEntry in responseResult.Rates)
        {
            var date = DateTime.Parse(dateEntry.Key, CultureInfo.InvariantCulture);
            foreach (var currencyEntry in dateEntry.Value)
            {
                result.Add(new RateValue
                {
                    DataSource = this.DataSourceName,
                    SourceCurrency = responseResult.BaseCurrency,
                    Currency = currencyEntry.Key,
                    CurrencyPairValue = responseResult.BaseCurrency + currencyEntry.Key,
                    Date = date,
                    Value = currencyEntry.Value,
                });
            }
        }

        return result.ToArray();
    }

    private async Task<RateValue[]> ExecuteLatestRatesRequest(string url)
    {
        var responseResult = await this.ExecuteProviderRequest<CurrencyBeaconLatestResponse>(url);

        if (responseResult.Error != null)
        {
#pragma warning disable CA1863
            var message = string.Format(CultureInfo.CurrentCulture, ProviderConstant.Exceptions.ExceptionRequestError, responseResult.Error.Code, responseResult.Error.Message);
#pragma warning restore CA1863
            throw new CrtException(message);
        }

        var date = DateTime.Parse(responseResult.Date, CultureInfo.InvariantCulture);

        return responseResult.Rates.Select(rate => new RateValue
        {
            DataSource = this.DataSourceName,
            SourceCurrency = responseResult.BaseCurrency,
            Currency = rate.Key,
            CurrencyPairValue = responseResult.BaseCurrency + rate.Key,
            Date = date,
            Value = rate.Value,
        }).ToArray();
    }

    private async Task<T> ExecuteProviderRequest<T>(string url)
    {
        using var httpClient = this.httpClientFactory.CreateClient();

        HttpResponseMessage response;
        string responseContent;

        lock (this.locker)
        {
            while (DateTime.Now.Subtract(this.lastRequestTime).Milliseconds < 100)
            {
            }

            response = httpClient.GetAsync(new Uri(url)).ConfigureAwait(false).GetAwaiter().GetResult();
            this.providerPeriodService.IncrementRequestCount(this.DataSourceNodeName);
            this.lastRequestTime = DateTime.Now;
        }

        this.providerPeriodService.SetLastSuccess(this.DataSourceNodeName, false);

        if (!response.IsSuccessStatusCode)
        {
            throw new CrtException($"{ProviderConstant.Exceptions.ExceptionRequestExecutionError} ({response.StatusCode})");
        }

        responseContent = await response.Content.ReadAsStringAsync();

        var responseResult = JsonConvert.DeserializeObject<T>(responseContent);

        if (responseResult == null)
        {
            throw new CrtException(ProviderConstant.Exceptions.ExceptionResponseParseError);
        }

        this.providerPeriodService.SetLastSuccess(this.DataSourceNodeName, true);

        return responseResult;
    }
}