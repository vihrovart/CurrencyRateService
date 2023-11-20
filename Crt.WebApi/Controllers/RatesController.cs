namespace Crt.WebApi.Controllers;

using Crt.Core.Models;
using Crt.Services;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Контроллер курсов валют.
/// </summary>
[ApiController]
[Route("[controller]")]
public class RatesController : Controller
{
    private readonly RateService rateService;

    /// <summary>
    /// Initializes a new instance of the <see cref="RatesController"/> class.
    /// </summary>
    /// <param name="rateService">Сервис курсов.</param>
    public RatesController(RateService rateService)
    {
        this.rateService = rateService;
    }

    /// <summary>
    /// Получить историю курсов валют относительно указанной валюты..
    /// </summary>
    /// <param name="startDate">Дата начала.</param>
    /// <param name="endDate">Дата окончания.</param>
    /// <param name="currency">Валюта.</param>
    /// <returns>История курсов.</returns>
    [HttpGet("timeframe")]
    public Task<RateValue[]> GetTimeFrameRates(DateTime startDate, DateTime endDate, string currency)
    {
        return this.rateService.GetTimeFrameRates(startDate, endDate, currency);
    }
}