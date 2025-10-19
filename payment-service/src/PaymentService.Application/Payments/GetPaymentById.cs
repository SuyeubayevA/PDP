using MediatR;
using PaymentService.Application.DTOs;
using PaymentService.Domain.Repositories;

namespace PaymentService.Application.Payments;

public record GetPaymentByIdQuery(int Id) : IRequest<PaymentDto?>;

public class GetPaymentByIdHandler : IRequestHandler<GetPaymentByIdQuery, PaymentDto?>
{
    private readonly IPaymentRepository _repo;
    public GetPaymentByIdHandler(IPaymentRepository repo) => _repo = repo;

    public async Task<PaymentDto?> Handle(GetPaymentByIdQuery request, CancellationToken ct)
    {
        var p = await _repo.GetByIdAsync(request.Id, ct);
        if (p is null) return null;

        return new PaymentDto
        {
            Id = p.Id,
            StudentId = p.StudentId,
            Amount = p.Amount,
            Currency = p.Currency,
            Period = p.Period,
            Status = p.Status,
            FailureReason = p.FailureReason,
            CreatedUtc = p.CreatedUtc
        };
    }
}
