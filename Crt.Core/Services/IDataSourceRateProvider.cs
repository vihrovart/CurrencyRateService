namespace Crt.Core.Services;

/// <summary>
/// Провайдер информации о курсах валют из источника.
/// </summary>
public interface IDataSourceRateProvider : IRateProvider
{
    /// <summary>
    /// Название источника данных.
    /// </summary>
    public string DataSourceName { get; }

    /// <summary>
    /// Название ноды источника данных.
    /// </summary>
    public string DataSourceNodeName { get; }

    /// <summary>
    /// Максимальная разница между датами промежутка.
    /// </summary>
    public int MaxDateDifference { get; }

    /// <summary>
    /// Количество запросов в сутки.
    /// </summary>
    public int DayRequestCount { get; }

    /// <summary>
    /// Количество запросов в сутки.
    /// </summary>
    public int MonthRequestCount { get; }
}