namespace StudentDiary.Application.Ports;

public interface IPaymentsService
{
    Task<bool> ProcessPaymentAsync(int studentId, decimal amount, string period, CancellationToken ct = default);
    Task<bool> FailPaymentAsync(CancellationToken ct = default);
    Task<bool> SlowPaymentAsync(CancellationToken ct = default);
}
