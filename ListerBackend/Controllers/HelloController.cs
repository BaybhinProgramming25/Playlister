using Microsoft.AspNetCore.Mvc;

namespace ListerBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeatherController : ControllerBase
{

    // Correlates to /api/weather
    [HttpGet]
    public IActionResult GetRoot()
    {
        return Ok("Hello from Root Application");
    }

    // Correlates to /api/weather/users
    [HttpGet("users")]
    public IActionResult GetAllUsers()
    {
        var users = new[]
        {
            new { id = 1, name = "John"},
            new { id = 2, name = "Matthew"}
        };
        return Ok(users);
    }

}