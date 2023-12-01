namespace Crt.Services;

using Crt.Core.Exceptions;
using Crt.Core.Models;
using Crt.Core.Services;
using Crt.Models;
using Crt.Constant;
using Microsoft.Extensions.Logging;

/// <summary>
/// Сервис курсов.
/// </summary>
public class RateService : IRateProvider
{
    private const int RepeatCount = 3;
    private const int RepeatIntervalMilliseconds = 500;

    private readonly ILogger<RateService> logger;
    private readonly RateProviderItem[] providers;

    /// <summary>
    /// Initializes a new instance of the <see cref="RateService"/> class.
    /// </summary>
    /// <param name="providers">Провайдеры курсов валют.</param>
    /// <param name="logger">Логгер.</param>
    public RateService(
        IEnumerable<IDataSourceRateProvider> providers,
        ILogger<RateService> logger)
    {
        this.logger = logger;
        this.providers = providers.Select(x => new RateProviderItem(x)).ToArray();
    }

    /// <inheritdoc/>
    public async Task<RateValue[]> GetTimeFrameRates(DateTime startDate, DateTime endDate, string currency)
    {
        var result = await this.ExecuteRequest(async x =>
        {
            var dateDifference = (endDate - startDate).Days + 1;

            var result = new List<RateValue>();

            var dates = Enumerable.Range(0, dateDifference).Select(x => startDate.AddDays(x)).ToArray();

            for (var i = 0; i < dates.Length; i += x.MaxDateDifference)
            {
                var range = dates.Skip(i).Take(x.MaxDateDifference).ToArray();

                var from = range.First();
                var to = range.Last();

                var requestResult = await x.GetTimeFrameRates(from, to, currency);
                result.AddRange(requestResult);
            }

            FillSelfCurrencyRate(result, x);

            return result.ToArray();
        });

        return result ?? Array.Empty<RateValue>();
    }

    /// <inheritdoc/>
    public async Task<RateValue[]> GetCurrentValue(string currency)
    {
        var result = await this.ExecuteRequest(async x =>
        {
            var result = (await x.GetCurrentValue(currency)).ToList();

            FillSelfCurrencyRate(result, x);

            return result.ToArray();
        });

        return result ?? Array.Empty<RateValue>();
    }

    private static void FillSelfCurrencyRate(List<RateValue> rates, IDataSourceRateProvider provider)
    {
        var dateGroups = rates.GroupBy(x => x.Date);

        foreach (var dateGroup in dateGroups)
        {
            var currencyGroups = dateGroup.GroupBy(x => x.SourceCurrency);

            rates.AddRange(from currencyGroup in currencyGroups
            where !currencyGroup.Any(x => x.Currency == currencyGroup.Key && x.Date == dateGroup.Key)
            select new RateValue()
            {
                Currency = currencyGroup.Key,
                SourceCurrency = currencyGroup.Key,
                Date = dateGroup.Key,
                Value = 1,
                DataSource = provider.DataSourceName,
                CurrencyPairValue = $"{currencyGroup.Key}{currencyGroup.Key}",
            });
        }
    }

    private static Task<T> ExecuteProviderRequest<T>(RateProviderItem providerItem, Func<IDataSourceRateProvider, Task<T>> requestFunc)
    {
        if (providerItem.LastRequestDate?.Date < DateTime.Now.Date)
        {
            providerItem.RequestCount = 0;
        }

        providerItem.RequestCount++;

        return requestFunc(providerItem.Provider);
    }

    private Task<T?> ExecuteRequest<T>(Func<IDataSourceRateProvider, Task<T>> requestFunc, int repeat = 0)
    {
        var provider = this.GetAvailableProvider();

        lock (provider)
        {
            var requestNumber = provider.RequestCount + 1;

            this.logger.LogDebug($"{provider.Provider.DataSourceName} provider, request #{requestNumber}/{provider.Provider.DayRequestCount}.");
            try
            {
                var result = ExecuteProviderRequest(provider, requestFunc).ConfigureAwait(false).GetAwaiter().GetResult();

                provider.LastRequestSuccess = true;
                provider.LastRequestDate = DateTime.Now.Date;

                this.logger.LogDebug($"{provider.Provider.DataSourceName} provider, request #{requestNumber}/{provider.Provider.DayRequestCount} - success.");

                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                provider.LastException = ex;
                provider.LastRequestDate = DateTime.Now.Date;
                provider.LastRequestSuccess = false;

                this.logger.LogDebug($"{provider.Provider.DataSourceName} provider, request #{provider.RequestCount} - fail.");

                if (repeat < RepeatCount)
                {
                    this.logger.LogDebug($"{provider.Provider.DataSourceName} provider, request #{provider.RequestCount} - try repeat #{repeat + 1}.");

                    Thread.Sleep(new TimeSpan(0, 0, 0, 0, RepeatIntervalMilliseconds));

                    return this.ExecuteRequest(requestFunc, repeat + 1);
                }

                throw;
            }
        }
    }

    private RateProviderItem GetAvailableProvider()
    {
        var provider = this.providers.FirstOrDefault(x => x.Provider.DayRequestCount > x.RequestCount && x.LastRequestSuccess);

        if (provider == null)
        {
            provider = this.providers.FirstOrDefault(x => x.Provider.DayRequestCount > x.RequestCount);
        }

        if (provider == null)
        {
            throw new CrtException(CrtConstant.Exceptions.AllProvidersCantExecuteRequest);
        }

        return provider;
    }
}