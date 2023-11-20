namespace Crt.Core.Services;

/// <summary>
/// Поставщик ключей.
/// </summary>
public interface IKeysProvider
{
    /// <summary>
    /// Получить ключ.
    /// </summary>
    /// <param name="dataSource">Источник данных.</param>
    /// <returns>Ключ.</returns>
    public string TryGetKey(string dataSource);
}