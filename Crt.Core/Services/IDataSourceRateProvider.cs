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
}