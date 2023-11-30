namespace Crt.Core.Models;

/// <summary>
/// Настройки провайдеров.
/// </summary>
public class ProvidersSettings
{
    /// <summary>
    /// Элементы.
    /// </summary>
    public IEnumerable<ProviderSettings> Items { get; set; }
}