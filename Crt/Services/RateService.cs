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

    private readonly ProviderPeriodService providerPeriodService;
    private readonly ILogger<RateService> logger;
    private readonly RateProviderItem[] providers;

    private int lastUserProviderIndex = -1;

    /// <summary>
    /// Initializes a new instance of the <see cref="RateService"/> class.
    /// </summary>
    /// <param name="providerPeriodService">Сервис для отслеживания количества запросов.</param>
    /// <param name="providers">Провайдеры курсов валют.</param>
    /// <param name="logger">Логгер.</param>
    public RateService(
        ProviderPeriodService providerPeriodService,
        IEnumerable<IDataSourceRateProvider> providers,
        ILogger<RateService> logger)
    {
        this.providerPeriodService = providerPeriodService;
        this.logger = logger;
        this.providers = providers.Select((x, i) => new RateProviderItem(x, i)).ToArray();
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

        return result ?? [];
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
        return requestFunc(providerItem.Provider);
    }

    private Task<T?> ExecuteRequest<T>(Func<IDataSourceRateProvider, Task<T>> requestFunc, int repeat = 0)
    {
        var provider = this.GetAvailableProvider();

        lock (provider)
        {
            var stat = this.providerPeriodService.GetStats(provider.Provider.DataSourceNodeName);
            var requestMonthNumber = stat.MonthCount + 1;
            var requestDayNumber = stat.DayCount + 1;
            var requestNumberString = $"D:{requestDayNumber}/{provider.Provider.DayRequestCount} M:{requestMonthNumber}/{provider.Provider.MonthRequestCount}";

            this.logger.LogDebug($"{provider.Provider.DataSourceName} provider, request #{requestNumberString}.");
            try
            {
                var result = ExecuteProviderRequest(provider, requestFunc).ConfigureAwait(false).GetAwaiter().GetResult();

                provider.LastRequestSuccess = true;
                provider.LastRequestDate = DateTime.Now.Date;

                this.logger.LogDebug($"{provider.Provider.DataSourceName} provider, request #{requestNumberString} - success.");

                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                provider.LastException = ex;
                provider.LastRequestDate = DateTime.Now.Date;
                provider.LastRequestSuccess = false;

                this.logger.LogDebug($"{provider.Provider.DataSourceName} provider, request #{requestNumberString} - fail.");

                if (repeat < RepeatCount)
                {
                    this.logger.LogDebug($"{provider.Provider.DataSourceName} provider, request #{requestNumberString} - try repeat #{repeat + 1}.");

                    Thread.Sleep(new TimeSpan(0, 0, 0, 0, RepeatIntervalMilliseconds));

                    return this.ExecuteRequest(requestFunc, repeat + 1);
                }

                throw;
            }
        }
    }

    private RateProviderItem GetAvailableProvider()
    {
        //// Если последний работоспособный то его беру.
        if (this.lastUserProviderIndex > -1
            && this.providerPeriodService
                .GetStats(this.providers[this.lastUserProviderIndex].Provider.DataSourceNodeName).WasLastSuccessful)
        {
            return this.providers[this.lastUserProviderIndex];
        }

        //// Пытаюсь найти работоспособный.

        //// Ищу не последний, но без ошибок.
        var provider = this.providers.FirstOrDefault(x =>
        {
            var stat = this.providerPeriodService.GetStats(x.Provider.DataSourceNodeName);
            return x.Index > this.lastUserProviderIndex
                   && (x.Provider.DayRequestCount <= 0 || x.Provider.DayRequestCount > stat.DayCount)
                   && (x.Provider.MonthRequestCount <= 0 || x.Provider.MonthRequestCount > stat.MonthCount)
                   && stat.WasLastSuccessful;
        });

        if (provider != null)
        {
            this.lastUserProviderIndex = provider.Index;
            return provider;
        }

        //// Ищу не последний, но можно и с ошибками.
        provider = this.providers.FirstOrDefault(x =>
        {
            var stat = this.providerPeriodService.GetStats(x.Provider.DataSourceNodeName);
            return x.Index > this.lastUserProviderIndex
                   && (x.Provider.DayRequestCount <= 0 || x.Provider.DayRequestCount > stat.DayCount)
                   && (x.Provider.MonthRequestCount <= 0 || x.Provider.MonthRequestCount > stat.MonthCount);
        });

        if (provider != null)
        {
            this.lastUserProviderIndex = provider.Index;
            return provider;
        }

        //// Если и такой не найден, то возвращаюсь к началу списка.
        if (provider == null)
        {
            this.lastUserProviderIndex = -1;
            throw new CrtException(CrtConstant.Exceptions.AllProvidersCantExecuteRequest);
        }

        return provider;
    }
}