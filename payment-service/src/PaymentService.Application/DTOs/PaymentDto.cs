using PaymentService.Domain.Entities;

namespace PaymentService.Application.DTOs;

public class PaymentDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "EUR";
    public string Period { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; }
    public string? FailureReason { get; set; }
    public DateTime CreatedUtc { get; set; }
}