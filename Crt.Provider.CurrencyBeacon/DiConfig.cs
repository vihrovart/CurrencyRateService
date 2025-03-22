namespace Crt.Provider.CurrencyBeacon;

using Crt.Core.Services;
using Crt.Provider.CurrencyBeacon.Services;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Регистрация зависимостей.
/// </summary>
public static class DiConfig
{
    /// <summary>
    /// Добавить провайдер курсов - CurrencyBeacon.
    /// </summary>
    /// <param name="services">Коллекция сервисов.</param>
    /// <returns>Коллекция сервисов - результат.</returns>
    public static IServiceCollection AddProviderCurrencyBeacon(this IServiceCollection services)
    {
        return services.AddSingleton<IDataSourceRateProvider, CurrencyBeaconDataSourceRateProvider>();
    }
}