namespace Crt.Provider.CurrencyBeacon.Constant;

/// <summary>
/// Общие константы, используемые провайдером CurrencyBeacon.
/// </summary>
#pragma warning disable CA1034
public static class ProviderConstant
{
    /// <summary>
    /// Формат даты в запросах.
    /// </summary>
    public const string RequestDateFormat = "yyyy-MM-dd";

    /// <summary>
    /// URL-адрес сервиса CurrencyBeacon (без протокола).
    /// </summary>
    private const string SiteUrl = "api.currencybeacon.com";

    /// <summary>
    /// Константы адресов.
    /// </summary>
    public static class Urls
    {
        /// <summary>
        /// Промежуток дат (timeframe).
        /// Параметры (в порядке подстановки):
        ///  0 - API ключ,
        ///  1 - базовая валюта,
        ///  2 - дата начала (в формате yyyy-MM-dd),
        ///  3 - дата окончания (в формате yyyy-MM-dd).
        /// </summary>
        public const string TimeFrame =
            $"https://{SiteUrl}/v1/timeframe?api_key={{0}}&base={{1}}&start_date={{2}}&end_date={{3}}";

        /// <summary>
        /// Текущее значение (latest).
        /// Параметры (в порядке подстановки):
        ///  0 - API ключ,
        ///  1 - базовая валюта.
        /// </summary>
        public const string Latest =
            $"https://{SiteUrl}/v1/latest?api_key={{0}}&base={{1}}";
    }

    /// <summary>
    /// Ошибки (сообщения и шаблоны исключений).
    /// </summary>
    public static class Exceptions
    {
#pragma warning disable SA1600 // (Если используете StyleCop, иначе можно убрать)
        public const string ExceptionNoSettingsForProvider = "No settings for provider";
        public const string ExceptionKeyFileNotFound = "Key file not found";
        public const string ExceptionRequestExecutionError = "Request execution error";
        public const string ExceptionResponseParseError = "Response parse error";
        public const string ExceptionRequestError = "Request error: {0} - {1}";
        public const string ExceptionRequestUnSupportedError = "Request unsupported error";
#pragma warning restore SA1600
    }
}
#pragma warning restore CA1034