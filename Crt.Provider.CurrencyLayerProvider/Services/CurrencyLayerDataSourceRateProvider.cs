namespace Crt.Provider.CurrencyLayerProvider.Services;

using System.Globalization;
using Crt.Core.Exceptions;
using Crt.Core.Models;
using Crt.Core.Services;
using Crt.Provider.CurrencyLayerProvider.Constant;
using Crt.Provider.CurrencyLayerProvider.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

/// <summary>
/// Провайдер курсов - CurrencyLayer.
/// </summary>
public class CurrencyLayerDataSourceRateProvider : IDataSourceRateProvider
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly ILogger<CurrencyLayerDataSourceRateProvider> logger;
    private readonly ProviderSettings settings;
    private readonly string key;

    private object locker = new object();

    private DateTime lastRequestTime;

    /// <summary>
    /// Initializes a new instance of the <see cref="CurrencyLayerDataSourceRateProvider"/> class.
    /// </summary>
    /// <param name="httpClientFactory">Фабрика http клиента.</param>
    /// <param name="settingsProvider">Поставщик настроек.</param>
    /// <param name="logger">Логгер.</param>
    public CurrencyLayerDataSourceRateProvider(
        IHttpClientFactory httpClientFactory,
        ISettingsProvider settingsProvider,
        ILogger<CurrencyLayerDataSourceRateProvider> logger)
    {
        this.lastRequestTime = DateTime.Now;
        this.httpClientFactory = httpClientFactory;
        this.logger = logger;

        this.settings = settingsProvider.TryGetSettings(this.DataSourceName) ?? throw new CrtException(ProviderConstant.Exceptions.ExceptionNoSettingsForProvider);

        if (!File.Exists(this.settings.KeyFile))
        {
            throw new CrtException(ProviderConstant.Exceptions.ExceptionKeyFileNotFound);
        }

        this.key = File.ReadAllText(this.settings.KeyFile);
    }

    /// <inheritdoc/>
    public string DataSourceName => "CurrencyLayer";

    /// <inheritdoc/>
    public int MaxDateDifference => this.settings.MaxDateDifference;

    /// <inheritdoc/>
    public int DayRequestCount => this.settings.DayRequestCount;

    /// <inheritdoc/>
    public Task<RateValue[]> GetTimeFrameRates(DateTime startDate, DateTime endDate, string currency)
    {
        var startDateParameter = startDate.ToString(ProviderConstant.RequestDateFormat, CultureInfo.CurrentCulture);
        var endDateParameter = endDate.ToString(ProviderConstant.RequestDateFormat, CultureInfo.CurrentCulture);

        var url = string.Format(
            CultureInfo.CurrentCulture,
            ProviderConstant.Urls.TimeFrame,
            this.key,
            startDateParameter,
            endDateParameter,
            currency);

        return Task.FromResult(this.ExecuteTimeFrameRatesRequest(url));
    }

    /// <inheritdoc/>
    public Task<RateValue[]> GetCurrentValue(string currency)
    {
        var url = string.Format(
            CultureInfo.CurrentCulture,
            ProviderConstant.Urls.Live,
            this.key,
            currency);

        return Task.FromResult(this.ExecuteLiveRatesRequest(url));
    }

    private static DateTime UnixTimeStampToDateTime(int unixTimeStamp)
    {
        // Unix timestamp is seconds past epoch
        var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
        return dateTime;
    }

    private RateValue[] ExecuteTimeFrameRatesRequest(string url)
    {
        var responseResult = this.ExecuteProviderRequest<TimeFrameResponse>(url);

        var data = responseResult.Quotes.SelectMany(
            x =>
                x.Value.Select(
                    y =>
                    {
                        var valueCurrency = y.Key.Substring(3, 3);

                        return new RateValue()
                        {
                            SourceCurrency = responseResult.Source,
                            DataSource = this.DataSourceName,
                            Date = x.Key,
                            Currency = valueCurrency,
                            CurrencyPairValue = y.Key,
                            Value = y.Value,
                        };
                    }));

        return data.ToArray();
    }

    private RateValue[] ExecuteLiveRatesRequest(string url)
    {
        var responseResult = this.ExecuteProviderRequest<LiveResponse>(url);

        var data = responseResult.Quotes.Select(
                    y =>
                    {
                        var valueCurrency = y.Key.Substring(3, 3);

                        return new RateValue()
                        {
                            SourceCurrency = responseResult.Source,
                            DataSource = this.DataSourceName,
                            Date = UnixTimeStampToDateTime(responseResult.TimeStamp),
                            Currency = valueCurrency,
                            CurrencyPairValue = y.Key,
                            Value = y.Value,
                        };
                    });

        return data.ToArray();
    }

    private T? ExecuteProviderRequest<T>(string url)
        where T : BaseQuoteResponse
    {
        using var httpClient = this.httpClientFactory.CreateClient();

        HttpResponseMessage? response;
        string responseContent;
        lock (this.locker)
        {
            while (DateTime.Now.Subtract(this.lastRequestTime).Milliseconds < 100)
            {
            }

            response = httpClient.GetAsync(new Uri(url)).ConfigureAwait(false).GetAwaiter().GetResult();

            this.lastRequestTime = DateTime.Now;

            if (!response.IsSuccessStatusCode)
            {
                throw new CrtException(
                    $"{ProviderConstant.Exceptions.ExceptionRequestExecutionError} ({response.StatusCode})");
            }

            responseContent = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        var responseResult = JsonConvert.DeserializeObject<T>(responseContent);

        if (responseResult == null)
        {
            throw new CrtException(ProviderConstant.Exceptions.ExceptionResponseParseError);
        }

        if (responseResult.Success)
        {
            return responseResult;
        }

        if (responseResult.Error == null)
        {
            throw new CrtException(ProviderConstant.Exceptions.ExceptionRequestUnSupportedError);
        }

        // TODO: Тут надо поработать над кодами ответов.
        var message = string.Format(
            CultureInfo.CurrentCulture,
            ProviderConstant.Exceptions.ExceptionRequestError,
            responseResult.Error.Code,
            responseResult.Error.Info);

        throw new CrtException(message);
    }
}