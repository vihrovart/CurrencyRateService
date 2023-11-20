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
    public Task<RateValue[]> GetTimeFrameRates(DateTime startDate, DateTime endDate, string currency)
    {
        foreach (var providerItem in this.providers)
        {
            try
            {
                var result =
                    ExecuteProviderRequest(providerItem, x => x.GetTimeFrameRates(startDate, endDate, currency));

                providerItem.LastRequestSuccess = true;
                providerItem.LastRequestDate = DateTime.Now.Date;

                return result;
            }
            catch (Exception ex)
            {
                providerItem.LastException = ex;
                providerItem.LastRequestDate = DateTime.Now.Date;
                providerItem.LastRequestSuccess = false;
            }
        }

        throw new CrtException(CrtConstant.Exceptions.AllProvidersCantExecuteRequest);
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
}