using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("")]
public class RootController : ControllerBase
{
    // Correlates to http://localhost:5144/
    [HttpGet]
    public IActionResult GetRoot()
    {
        return Ok("Joe Root is the Root of all evil");
    }

    // Correlates to http://localhost:5144/england
    [HttpGet("england")]
    public IActionResult GetEngland()
    {
        return Ok("The british are coming...");
    }

}