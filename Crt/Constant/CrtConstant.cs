namespace Crt.Constant;

/// <summary>
/// Константы проекта Crt.
/// </summary>
public static class CrtConstant
{
    /// <summary>
    /// Ошибки.
    /// </summary>
#pragma warning disable CA1034
    public static class Exceptions
#pragma warning restore CA1034
    {
#pragma warning disable SA1600
        public const string AllProvidersCantExecuteRequest = "All providers can't execute request";
#pragma warning restore SA1600
    }
}