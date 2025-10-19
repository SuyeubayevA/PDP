namespace PaymentService.Api.Models;

public class ProcessPaymentRequest
{
    public int StudentId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "EUR";
    public string Period { get; set; } = string.Empty;
}
