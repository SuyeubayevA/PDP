using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using School.Contracts;
using StudentDiary.Application.Students;
using StudentDiary.Domain.Entities;
using StudentDiary.Domain.Repositories;
using StudentDiary.Infrastructure.Repositories;
using StudentDiary.Presentation.Models;
using System.Text.Json;

namespace StudentDiary.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    private readonly IMediator _mediator;
    public StudentsController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStudentRequest request)
    {
        var id = await _mediator.Send(new CreateStudentCommand(request.FullName, request.Class));
        return CreatedAtAction(nameof(GetById), new { id }, null);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var student = await _mediator.Send(new GetStudentQuery(id));
        if (student == null) return NotFound();
        return Ok(student);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var students = await _mediator.Send(new GetAllStudentsQuery());
        return Ok(students);
    }

    [HttpPost("{id:int}/grades")]
    public async Task<IActionResult> UpsertGrade(int id, [FromBody] UpsertGradeRequest request)
    {
        await _mediator.Send(new UpsertGradeCommand(id, request.Subject, request.Value));
        return NoContent();
    }

    [HttpPost("{studentId:int}/activate")]
    public async Task<IActionResult> ActivateStudent(int studentId, [FromBody] ActivateStudentRequest body)
    {
        var result = await _mediator.Send(new ActivateStudentCommand(studentId, body.Amount, body.Period, body.PaymentType));
        return result
            ? Ok("Succesful payment, student is active")
            : BadRequest("Payment was failed");
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
    int id,
    [FromServices] IStudentRepository repo,
    [FromServices] OutboxRepository outboxRepo,
    CancellationToken ct)
    {
        var student = await repo.GetByIdAsync(id, ct);
        if (student is null) return NotFound();

        // removes student
        await repo.DeleteAsync(student, ct);

        // write in Outbox
        var evt = new StudentRemoved(id);
        var message = new OutboxMessage
        {
            Type = nameof(StudentRemoved),
            Payload = JsonSerializer.Serialize(evt),
            CreatedUtc = DateTime.UtcNow
        };

        await outboxRepo.AddAsync(message, ct);
        await outboxRepo.SaveChangesAsync(ct);

        return NoContent();
    }
}
