using MassTransit;
using Microsoft.AspNetCore.Mvc;
using School.Contracts;

namespace StudentDiary.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ActivitiesController : ControllerBase
{
    private readonly IPublishEndpoint _bus;
    public ActivitiesController(IPublishEndpoint bus) => _bus = bus;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateActivityRequest req, CancellationToken ct)
    {
        // теперь Id не генерируем, просто публикуем
        var evt = new ActivityCreated(
            Name: req.Name,
            Schedule: req.Schedule,
            Capacity: req.Capacity
        );

        await _bus.Publish(evt, ct);
        return Ok("Activity creation event published");
    }

    [HttpPost("{id:int}/enroll")]
    public async Task<IActionResult> Enroll(int id, [FromBody] EnrollStudentRequest req, CancellationToken ct)
    {
        var evt = new StudentEnrolledInActivity(id, req.StudentId);
        await _bus.Publish(evt, ct);
        return Ok("Student enrollment event published");
    }
}

public record CreateActivityRequest(string Name, string Schedule, int Capacity);
public record EnrollStudentRequest(int StudentId);
