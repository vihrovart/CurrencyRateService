namespace Crt.Provider.Frankfurter;

using Crt.Core.Services;
using Crt.Provider.Frankfurter.Services;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Регистрация зависимостей.
/// </summary>
public static class DiConfig
{
    /// <summary>
    /// Добавить провайдер курсов - Frankfurter.
    /// </summary>
    /// <param name="services">Коллекция сервисов.</param>
    /// <returns>Коллекция сервисов - результат.</returns>
    public static IServiceCollection AddProviderFrankfurter(this IServiceCollection services)
    {
        return services.AddSingleton<IDataSourceRateProvider, FrankfurterDataSourceRateProvider>();
    }
}