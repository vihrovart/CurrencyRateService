namespace Crt.Core.Services;

using System;
using System.Threading.Tasks;
using Crt.Core.Models;

/// <summary>
/// Поставщик данных о курсах валют.
/// </summary>
public interface IRateProvider
{
    /// <summary>
    /// Получить исторические значения курсов валют, относительно указанной валюты.
    /// </summary>
    /// <param name="startDate">Дата начала.</param>
    /// <param name="endDate">Дата окончания.</param>
    /// <param name="currency">Валюта.</param>
    /// <returns>Исторические значения курсов.</returns>
    public Task<RateValue[]> GetTimeFrameRates(DateTime startDate, DateTime endDate, string currency);

    /// <summary>
    /// Получить текущее значение курсов относительно указанной валюты.
    /// </summary>
    /// <param name="currency">Валюта.</param>
    /// <returns>Значения курсов.</returns>
    public RateValue[] GetCurrentValue(string currency);
}