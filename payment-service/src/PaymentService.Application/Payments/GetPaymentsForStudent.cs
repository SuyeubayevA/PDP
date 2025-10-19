using MediatR;
using PaymentService.Application.DTOs;
using PaymentService.Domain.Repositories;

namespace PaymentService.Application.Payments;

public record GetPaymentsForStudentQuery(int StudentId) : IRequest<List<PaymentDto>>;

public class GetPaymentsForStudentHandler : IRequestHandler<GetPaymentsForStudentQuery, List<PaymentDto>>
{
    private readonly IPaymentRepository _repo;
    public GetPaymentsForStudentHandler(IPaymentRepository repo) => _repo = repo;

    public async Task<List<PaymentDto>> Handle(GetPaymentsForStudentQuery request, CancellationToken ct)
    {
        var list = await _repo.ListByStudentAsync(request.StudentId, ct);
        return list.Select(p => new PaymentDto
        {
            Id = p.Id,
            StudentId = p.StudentId,
            Amount = p.Amount,
            Currency = p.Currency,
            Period = p.Period,
            Status = p.Status,
            FailureReason = p.FailureReason,
            CreatedUtc = p.CreatedUtc
        }).ToList();
    }
}
