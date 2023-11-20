namespace Crt.Core.Models;

using System.Collections.Generic;

/// <summary>
/// Настройки провайдеров.
/// </summary>
public class ProviderSettings
{
    /// <summary>
    /// Ключи сервисов.
    /// </summary>
    public Dictionary<string, string> Keys { get; set; }
}