namespace Crt.Services;

using Crt.Core.Exceptions;
using Crt.Core.Models;
using Crt.Core.Services;
using Crt.Models;
using Crt.Constant;

/// <summary>
/// Сервис курсов.
/// </summary>
public class RateService : IRateProvider
{
    private readonly RateProviderItem[] providers;

    /// <summary>
    /// Initializes a new instance of the <see cref="RateService"/> class.
    /// </summary>
    /// <param name="providers">Провайдеры курсов валют.</param>
    public RateService(IEnumerable<IDataSourceRateProvider> providers)
    {
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

            return result.ToArray();
        });

        return result ?? Array.Empty<RateValue>();
    }

    /// <inheritdoc/>
    public RateValue[] GetCurrentValue(string currency)
    {
        throw new NotImplementedException();
    }

    private static Task<T> ExecuteProviderRequest<T>(RateProviderItem providerItem, Func<IDataSourceRateProvider, Task<T>> requestFunc)
    {
        if (providerItem.LastRequestDate?.Date < DateTime.Now)
        {
            providerItem.RequestCount = 0;
        }

        providerItem.RequestCount++;

        return requestFunc(providerItem.Provider);
    }

    private async Task<T?> ExecuteRequest<T>(Func<IDataSourceRateProvider, Task<T>> requestFunc)
    {
        var provider = this.GetAvailableProvider();

        if (provider == null)
        {
            throw new CrtException(CrtConstant.Exceptions.AllProvidersCantExecuteRequest);
        }

        T? result = default;

        try
        {
            result = await ExecuteProviderRequest(provider, requestFunc);

            provider.LastRequestSuccess = true;
            provider.LastRequestDate = DateTime.Now.Date;

            return result;
        }
        catch (Exception ex)
        {
            provider.LastException = ex;
            provider.LastRequestDate = DateTime.Now.Date;
            provider.LastRequestSuccess = false;
        }

        return result;
    }

    private RateProviderItem? GetAvailableProvider()
    {
        return this.providers.FirstOrDefault(x => x.Provider.DayRequestCount > x.RequestCount);
    }
}