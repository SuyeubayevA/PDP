using StudentDiary.Application.Ports;
using System.Net.Http.Json;

namespace StudentDiary.Infrastructure.Integrations;

public class PaymentsApiClient : IPaymentsService
{
    private readonly HttpClient _http;
    public PaymentsApiClient(HttpClient http) => _http = http;

    public async Task<bool> ProcessPaymentAsync(int studentId, decimal amount, string period, CancellationToken ct = default)
    {
        var request = new
        {
            studentId,
            amount,
            currency = "EUR",
            period
        };

        var response = await _http.PostAsJsonAsync("api/payments", request, ct);

        response.EnsureSuccessStatusCode();
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> FailPaymentAsync(CancellationToken ct = default)
    {
        var response = await _http.GetAsync("api/test/fail", ct);

        response.EnsureSuccessStatusCode();
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> SlowPaymentAsync(CancellationToken ct = default)
    {
        var response = await _http.GetAsync("api/test/slow", ct);

        response.EnsureSuccessStatusCode();
        return response.IsSuccessStatusCode;
    }
}
