using PaymentService.Application.Ports;

namespace PaymentService.Infrastructure.Adapters;

public class FakePaymentGateway : IPaymentGateway
{
    public Task<PaymentGatewayResult> ChargeAsync(int studentId, decimal amount, string currency, CancellationToken ct)
    {
        if (amount <= 0m) return Task.FromResult(new PaymentGatewayResult(false, "Amount must be > 0"));
        if (amount == 13.37m) return Task.FromResult(new PaymentGatewayResult(false, "Test decline"));
        return Task.FromResult(new PaymentGatewayResult(true, null));
    }
}
