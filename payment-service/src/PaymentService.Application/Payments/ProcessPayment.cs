using MediatR;
using PaymentService.Application.DTOs;
using PaymentService.Application.Ports;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Repositories;

namespace PaymentService.Application.Payments;

public record ProcessPaymentCommand(int StudentId, decimal Amount, string Currency, string Period)
        : IRequest<PaymentDto>;

public class ProcessPaymentHandler : IRequestHandler<ProcessPaymentCommand, PaymentDto>
{
    private readonly IPaymentRepository _repo;
    private readonly IPaymentGateway _gateway;

    public ProcessPaymentHandler(IPaymentRepository repo, IPaymentGateway gateway)
    {
        _repo = repo;
        _gateway = gateway;
    }

    public async Task<PaymentDto> Handle(ProcessPaymentCommand request, CancellationToken ct)
    {
        if (request.Amount <= 0) throw new ArgumentException("Amount must be > 0");
        if (string.IsNullOrWhiteSpace(request.Currency)) throw new ArgumentException("Currency is required");
        if (string.IsNullOrWhiteSpace(request.Period)) throw new ArgumentException("Period is required");

        var payment = new Payment
        {
            StudentId = request.StudentId,
            Amount = request.Amount,
            Currency = request.Currency,
            Period = request.Period,
            Status = PaymentStatus.Pending,
            CreatedUtc = DateTime.UtcNow
        };

        await _repo.AddAsync(payment, ct);
        await _repo.SaveChangesAsync(ct);

        var result = await _gateway.ChargeAsync(request.StudentId, request.Amount, request.Currency, ct);

        if (result.Success)
        {
            payment.Status = PaymentStatus.Succeeded;
            payment.FailureReason = null;
        }
        else
        {
            payment.Status = PaymentStatus.Failed;
            payment.FailureReason = result.Error ?? "Error";
        }

        await _repo.SaveChangesAsync(ct);

        return new PaymentDto
        {
            Id = payment.Id,
            StudentId = payment.StudentId,
            Amount = payment.Amount,
            Currency = payment.Currency,
            Period = payment.Period,
            Status = payment.Status,
            FailureReason = payment.FailureReason,
            CreatedUtc = payment.CreatedUtc
        };
    }
}
