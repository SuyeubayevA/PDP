using MediatR;
using Microsoft.AspNetCore.Mvc;
using ActivitiesService.Application.Activities;

namespace ActivitiesService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ActivitiesController : ControllerBase
{
    private readonly IMediator _mediator;
    public ActivitiesController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateActivityRequest req, CancellationToken ct)
    {
        var id = await _mediator.Send(new CreateActivityCommand(req.Name, req.Schedule, req.Capacity), ct);
        return CreatedAtAction(nameof(GetEnrollments), new { id }, new { id });
    }

    [HttpPost("{id:int}/enroll")]
    public async Task<IActionResult> Enroll(int id, [FromBody] EnrollRequest req, CancellationToken ct)
    {
        var enrId = await _mediator.Send(new EnrollStudentCommand(id, req.StudentId), ct);
        return Ok(new { enrollmentId = enrId });
    }

    [HttpGet("{id:int}/enrollments")]
    public Task<List<object>> GetEnrollments(int id, CancellationToken ct) =>
        _mediator.Send(new ListEnrollmentsQuery(id), ct)
                 .ContinueWith(t => t.Result.Select(e => new { e.Id, e.StudentId, e.Status, e.EnrolledUtc }).Cast<object>().ToList(), ct);

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var activities = await _mediator.Send(new ListActivitiesQuery(), ct);
        return Ok(activities.Select(a => new
        {
            a.Id,
            a.Name,
            a.Schedule,
            a.Capacity,
            a.IsActive
        }));
    }
}

public record CreateActivityRequest(string Name, string Schedule, int Capacity);
public record EnrollRequest(int StudentId);
