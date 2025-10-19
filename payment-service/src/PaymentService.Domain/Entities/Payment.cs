namespace PaymentService.Domain.Entities;

public enum PaymentStatus
{
    Pending = 0,
    Succeeded = 1,
    Failed = 2
}

public class Payment
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
