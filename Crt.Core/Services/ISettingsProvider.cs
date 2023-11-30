namespace Crt.Core.Services;

using Crt.Core.Models;

/// <summary>
/// Поставщик ключей.
/// </summary>
public interface ISettingsProvider
{
    /// <summary>
    /// Получить настройки провайдера.
    /// </summary>
    /// <param name="dataSource">Источник данных.</param>
    /// <returns>Ключ.</returns>
    public ProviderSettings? TryGetSettings(string dataSource);
}