namespace Crt.Services;

using Crt.Core.Models;
using Crt.Core.Services;
using Microsoft.Extensions.Options;

/// <summary>
/// Поставщик ключей.
/// </summary>
public class SettingsProvider : ISettingsProvider
{
    private readonly ProvidersSettings settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsProvider"/> class.
    /// </summary>
    /// <param name="settings">Настройки провайдеров.</param>
    public SettingsProvider(IOptions<ProvidersSettings> settings)
    {
        this.settings = settings.Value;
    }

    /// <inheritdoc/>
    public ProviderSettings? TryGetSettings(string dataSource)
    {
        return this.settings.Items.FirstOrDefault(x => x.ProviderName == dataSource);
    }
}