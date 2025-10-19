namespace StudentDiary.Presentation.Models;

public class UpsertGradeRequest
{
    public string Subject { get; set; } = string.Empty;
    public int Value { get; set; }
}
