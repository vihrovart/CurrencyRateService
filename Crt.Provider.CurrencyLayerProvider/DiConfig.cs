namespace Crt.Provider.CurrencyLayerProvider;

using Crt.Core.Services;
using Crt.Provider.CurrencyLayerProvider.Services;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Регистрация зависимостей.
/// </summary>
public static class DiConfig
{
    /// <summary>
    /// Добавить провайдер курсов - Currency layer.
    /// </summary>
    /// <param name="services">Коллекция сервисов.</param>
    /// <returns>Коллекция сервисов - результат.</returns>
    public static IServiceCollection AddProviderCurrencyLayer(this IServiceCollection services)
    {
        return services.AddSingleton<IDataSourceRateProvider, CurrencyLayerDataSourceRateProvider>();
    }
}