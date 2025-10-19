namespace StudentDiary.Domain.Entities;

public class Grade
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string Subject { get; set; } = default!;
    public int Value { get; set; }
    public DateTime Date { get; set; }
}
