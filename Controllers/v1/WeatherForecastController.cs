using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Suspense.Server.Controllers.v1;

/// <summary>
/// Test controller.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries =
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    /// <summary>
    /// Return A mock list of different temperatures.
    /// </summary>
    [HttpGet]
    public IActionResult Get()
    {
        var rng = new Random();

        var temperatures = Enumerable.Range(1, 5).Select(index => new
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();

        return Ok(temperatures);
    }
}