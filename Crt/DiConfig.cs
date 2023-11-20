namespace Crt;

using Crt.Core.Services;
using Crt.Services;
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
    /// <returns>Коллекция сервисов - результат.</returns>
    public static IServiceCollection AddCrt(this IServiceCollection services)
    {
        return services
            .AddSingleton<IKeysProvider, KeysProvider>()
            .AddSingleton<RateService>();
    }
}