using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
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

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        _logger.LogInformation($"Test Serilog and Elastic Stack");

        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
    }

    [HttpPost]
    public IActionResult Post([FromBody] WeatherForecast? weatherForecast)
    {
        if (weatherForecast?.Date < DateTime.Now)
        {
            _logger.LogWarning("Weather Forecast it's null");
        }
        
        return Ok(weatherForecast);
    }
    
    [HttpPut]
    public IActionResult Put([FromBody] WeatherForecast? weatherForecast)
    {
        try
        {
            if (weatherForecast?.Date < DateTime.Now)
                throw new Exception("Weather Forecast it's null");
        }
        catch (Exception ex)
        {
            _logger.LogError("Log error: {e}",ex.Message);
        }
        
        return Ok(weatherForecast);
    }
}
