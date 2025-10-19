using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Api.Models;
using PaymentService.Application.Payments;

namespace PaymentService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;
    public PaymentsController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<IActionResult> Process([FromBody] ProcessPaymentRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new ProcessPaymentCommand(req.StudentId, req.Amount, req.Currency, req.Period), ct);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var payment = await _mediator.Send(new GetPaymentByIdQuery(id), ct);
        return payment is null ? NotFound() : Ok(payment);
    }

    [HttpGet("student/{studentId:int}")]
    public async Task<IActionResult> GetForStudent(int studentId, CancellationToken ct)
    {
        var list = await _mediator.Send(new GetPaymentsForStudentQuery(studentId), ct);
        return Ok(list);
    }
}
