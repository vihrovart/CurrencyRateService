namespace Crt.Provider.CurrencyBeacon.Models;

using Newtonsoft.Json;

/// <summary>
/// Модель описания ошибки у Currency Beacon.
/// </summary>
public class CurrencyBeaconError
{
    /// <summary>
    /// Код ошибки.
    /// </summary>
    [JsonProperty("code")]
    public string Code { get; set; }

    /// <summary>
    /// Сообщение об ошибке.
    /// </summary>
    [JsonProperty("message")]
    public string Message { get; set; }
}