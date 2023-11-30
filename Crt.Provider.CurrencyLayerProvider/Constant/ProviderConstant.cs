namespace Crt.Provider.CurrencyLayerProvider.Constant;

/// <summary>
/// Константы провайдера.
/// </summary>
internal static class ProviderConstant
{
    /// <summary>
    /// Формат даты в запросах.
    /// </summary>
    public const string RequestDateFormat = "yyyy-MM-dd";

    private const string SiteUrl = "api.currencylayer.com";

    /// <summary>
    /// Константы адресов.
    /// </summary>
    public static class Urls
    {
        /// <summary>
        /// Историческое значение.
        /// 0 - ключ.
        /// 1 - дата начала.
        /// 2 - дата окончания.
        /// 3 - валюта.
        /// </summary>
        public const string TimeFrame =
            $"http://{SiteUrl}/timeframe?access_key={{0}}&start_date={{1}}&end_date={{2}}&source={{3}}";
    }

    /// <summary>
    /// Ошибки.
    /// </summary>
    public static class Exceptions
    {
#pragma warning disable SA1600
        public const string ExceptionNoSettingsForProvider = "No settings for provider";
        public const string ExceptionKeyFileNotFound = "Key file not found";
        public const string ExceptionRequestExecutionError = "Request execution error";
        public const string ExceptionResponseParseError = "Response parse error";
        public const string ExceptionRequestError = "Request error: {0} - {1}";
        public const string ExceptionRequestUnSupportedError = "Request unsuppoted error";
#pragma warning restore SA1600
    }
}