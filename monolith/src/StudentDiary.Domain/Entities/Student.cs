namespace StudentDiary.Domain.Entities;

public class Student
{
    public int Id { get; set; }
    public string FullName { get; set; } = default!;
    public string Class { get; set; } = default!;
    public List<Grade> Grades { get; set; } = new();
    public bool IsActive { get; set; } = false;
}
