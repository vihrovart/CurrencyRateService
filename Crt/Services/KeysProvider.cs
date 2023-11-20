namespace Crt.Services;

using Crt.Core.Models;
using Crt.Core.Services;
using Microsoft.Extensions.Options;

/// <summary>
/// Поставщик ключей.
/// </summary>
public class KeysProvider : IKeysProvider
{
    private readonly ProviderSettings settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="KeysProvider"/> class.
    /// </summary>
    /// <param name="settings">Настройки провайдеров.</param>
    public KeysProvider(IOptions<ProviderSettings> settings)
    {
        this.settings = settings.Value;
    }

    /// <inheritdoc/>
    public string TryGetKey(string dataSource)
    {
        return this.settings.Keys.TryGetValue(dataSource, out var key) ? key : string.Empty;
    }
}