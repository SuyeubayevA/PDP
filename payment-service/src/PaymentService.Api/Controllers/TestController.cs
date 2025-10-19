using Microsoft.AspNetCore.Mvc;

namespace PaymentService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [HttpGet("fail")]
    public IActionResult Fail()
    {
        return StatusCode(500, "Simulated failure");
    }

    [HttpGet("slow")]
    public async Task<IActionResult> Slow()
    {
        await Task.Delay(5000);
        return Ok("Simulated slow response");
    }

    [HttpGet("ok")]
    public IActionResult OkTest()
    {
        return Ok("All good");
    }
}
