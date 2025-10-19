using StudentDiary.Application.Students;

namespace StudentDiary.Presentation.Models;

public class ActivateStudentRequest
{
    public decimal Amount { get; set; }
    public string Period { get; set; } = string.Empty;
    public PaymentType PaymentType { get; set; } = PaymentType.Normal;

}
