namespace Crt.Models;

using System;
using Crt.Core.Services;

/// <summary>
/// Элемент провайдера.
/// </summary>
internal class RateProviderItem
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RateProviderItem"/> class.
    /// </summary>
    /// <param name="provider">Провайдер курсов валют.</param>
    public RateProviderItem(IDataSourceRateProvider provider)
    {
        this.Provider = provider;
    }

    /// <summary>
    /// Поставщик данных.
    /// </summary>
    public IDataSourceRateProvider Provider { get; set; }

    /// <summary>
    /// Последний запрос выполнен с успехом.
    /// </summary>
    public bool LastRequestSuccess { get; set; }

    /// <summary>
    /// Количество выполненных запросов.
    /// </summary>
    public int RequestCount { get; set; }

    /// <summary>
    /// Дата последнего запроса.
    /// </summary>
    public DateTime? LastRequestDate { get; set; }

    /// <summary>
    /// Последняя ошибка.
    /// </summary>
    public Exception? LastException { get; set; }
}