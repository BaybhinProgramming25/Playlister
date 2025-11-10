using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("/[controller]")]
public class HomeController : ControllerBase
{
    // Correlates to http:localhost:5144/home
    [HttpGet]
    public IActionResult GetHome()
    {
        return Ok("Hello from http://localhost:5144/");
    }

    // Correlates to http:localhost:5144/home/animals
    [HttpGet("animals")]
    public IActionResult GetAnimals()
    {
        var animals = new[]
        {
            new { id = 1, type = "Dog"},
            new { id = 2, type = "Cat"},
            new { id = 3, type = "Fish"}
        };

        return Ok(animals);
    }
}