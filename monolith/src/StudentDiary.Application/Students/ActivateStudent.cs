using MediatR;
using StudentDiary.Application.Ports;
using StudentDiary.Domain.Repositories;

namespace StudentDiary.Application.Students;

public enum PaymentType
{
    Normal,
    Fail,
    Slowly
}

public record ActivateStudentCommand(int StudentId, decimal Amount, string Period, PaymentType Type = PaymentType.Normal) : IRequest<bool>;

public class ActivateStudentHandler : IRequestHandler<ActivateStudentCommand, bool>
{
    private readonly IStudentRepository _repo;
    private readonly IPaymentsService _payments;

    public ActivateStudentHandler(IStudentRepository repo, IPaymentsService payments)
    {
        _repo = repo;
        _payments = payments;
    }

    public async Task<bool> Handle(ActivateStudentCommand request, CancellationToken ct)
    {
        var student = await _repo.GetByIdAsync(request.StudentId, ct);
        if (student is null) return false;

        switch (request.Type)
        {
            case PaymentType.Fail:
                await _payments.FailPaymentAsync();
                return false;

            case PaymentType.Slowly:
                await _payments.SlowPaymentAsync();
                return false;

            case PaymentType.Normal:
                var paid = await _payments.ProcessPaymentAsync(request.StudentId, request.Amount, request.Period, ct);
                if (!paid) return false;

                student.IsActive = true;
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        //var paid = await _payments.ProcessPaymentAsync(request.StudentId, request.Amount, request.Period, ct);
        //if (!paid) return false;

        //student.IsActive = true;
        await _repo.UpdateAsync(student, ct);

        return true;
    }
}
