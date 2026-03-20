using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WeatherApiController : ControllerBase
    {
        // Simple sample DTO
        public record DayForecast(string Date, int TemperatureC, string Summary);

        [HttpGet]
        public IActionResult Get7DayForecast()
        {
            var rng = new Random();
            var data = Enumerable.Range(0, 7).Select(i =>
                new DayForecast(
                    DateTime.Today.AddDays(i).ToString("yyyy-MM-dd"),
                    10 + rng.Next(15),
                    i % 3 == 0 ? "Sunny" : (i % 3 == 1 ? "Cloudy" : "Rain")
                )
            );

            return Ok(data);
        }
    }
}
