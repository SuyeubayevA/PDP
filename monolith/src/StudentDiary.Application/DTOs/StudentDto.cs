namespace StudentDiary.Application.DTOs;

public class StudentDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = default!;
    public string Class { get; set; } = default!;
    public List<GradeDto> Grades { get; set; } = new();
    public bool IsActive { get; set; }
}

public class GradeDto
{
    public int Id { get; set; }
    public string Subject { get; set; } = default!;
    public int Value { get; set; }
    public DateTime Date { get; set; }
}
