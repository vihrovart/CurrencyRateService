namespace Crt.Provider.CurrencyLayerProvider.Services;

using System.Globalization;
using Crt.Core.Exceptions;
using Crt.Core.Models;
using Crt.Core.Services;
using Crt.Provider.CurrencyLayerProvider.Constant;
using Crt.Provider.CurrencyLayerProvider.Models;
using Newtonsoft.Json;

/// <summary>
/// Провайдер курсов - CurrencyLayer.
/// </summary>
public class CurrencyLayerDataSourceRateProvider : IDataSourceRateProvider
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly string key;

    /// <summary>
    /// Initializes a new instance of the <see cref="CurrencyLayerDataSourceRateProvider"/> class.
    /// </summary>
    /// <param name="httpClientFactory">Фабрика http клиента.</param>
    /// <param name="keysProvider">Поставщик ключей.</param>
    public CurrencyLayerDataSourceRateProvider(
        IHttpClientFactory httpClientFactory,
        IKeysProvider keysProvider)
    {
        this.httpClientFactory = httpClientFactory;
        this.key = keysProvider.TryGetKey(this.DataSourceName);

        if (string.IsNullOrEmpty(this.key))
        {
            throw new CrtException(ProviderConstant.Exceptions.ExceptionCantGetApiKey);
        }
    }

    /// <inheritdoc/>
    public string DataSourceName => "CurrencyLayer";

    /// <inheritdoc/>
    public async Task<RateValue[]> GetTimeFrameRates(DateTime startDate, DateTime endDate, string currency)
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

        using var httpClient = this.httpClientFactory.CreateClient();

        var response = await httpClient.GetAsync(new Uri(url));

        if (!response.IsSuccessStatusCode)
        {
            throw new CrtException(
                $"{ProviderConstant.Exceptions.ExceptionRequestExecutionError} ({response.StatusCode})");
        }

        var responseContent = await response.Content.ReadAsStringAsync();

        var responseResult = JsonConvert.DeserializeObject<TimeFrameResponse>(responseContent);

        if (responseResult == null)
        {
            throw new CrtException(ProviderConstant.Exceptions.ExceptionResponseParseError);
        }

        if (!responseResult.Success)
        {
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
                        Value = y.Value,
                    };
                }));

        return data.ToArray();
    }

    /// <inheritdoc/>
    public RateValue[] GetCurrentValue(string currency)
    {
        throw new NotImplementedException();
    }
}