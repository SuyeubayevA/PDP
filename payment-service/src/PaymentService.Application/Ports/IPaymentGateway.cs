namespace PaymentService.Application.Ports;

public record PaymentGatewayResult(bool Success, string? Error);

public interface IPaymentGateway
{
    Task<PaymentGatewayResult> ChargeAsync(int studentId, decimal amount, string currency, CancellationToken ct);
}
