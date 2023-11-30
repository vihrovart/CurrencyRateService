namespace Crt.Core.Models;

using System;

/// <summary>
/// Значение курса.
/// </summary>
public class RateValue
{
    /// <summary>
    /// Исходная валюта.
    /// </summary>
    public string SourceCurrency { get; set; }

    /// <summary>
    /// Валюта.
    /// </summary>
    public string Currency { get; set; }

    /// <summary>
    /// Значение пары валют.
    /// </summary>
    public string CurrencyPairValue { get; set; }

    /// <summary>
    /// Дата.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Значение.
    /// </summary>
    public decimal Value { get; set; }

    /// <summary>
    /// Источник данных.
    /// </summary>
    public string DataSource { get; set; }
}