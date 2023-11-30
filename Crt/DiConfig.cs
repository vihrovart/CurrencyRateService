namespace Crt;

using Crt.Core.Models;
using Crt.Core.Services;
using Crt.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Регистрация зависимостей.
/// </summary>
public static class DiConfig
{
    /// <summary>
    /// Добавить поддержку Crt.
    /// </summary>
    /// <param name="services">Коллекция сервисов.</param>
    /// <param name="configuration">Конфигурация.</param>
    /// <returns>Коллекция сервисов - результат.</returns>
    public static IServiceCollection AddCrt(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .Configure<ProvidersSettings>(configuration.GetSection(nameof(ProvidersSettings)))
            .AddSingleton<ISettingsProvider, SettingsProvider>()
            .AddSingleton<RateService>();
    }
}