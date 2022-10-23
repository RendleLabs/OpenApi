using Microsoft.AspNetCore.Mvc;

namespace Testing.WebApi.Controllers;

[ApiController]
public abstract class WeatherForecastControllerBase : ControllerBase
{
    [HttpGet("WeatherForecast", Name = "GetWeatherForecast")]
    public virtual ActionResult<IEnumerable<WeatherForecast>> Get()
    {
        return StatusCode(501);
    }
}

public class WeatherForecastController : WeatherForecastControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    public override ActionResult<IEnumerable<WeatherForecast>> Get()
    {
        Response.Headers.Location = Url.Action("Get", "WeatherForecast");
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateTime.Now.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }
}
