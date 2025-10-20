using StudentDiary.Domain.Entities;

namespace StudentDiary.Infrastructure.Data;

public static class DbInitializer
{
    public static void Seed(StudentDiaryDbContext context)
    {
        if (context.Students.Any()) return;

        var student1 = new Student { FullName = "Hamilton Santos", Class = "5A" };
        student1.Grades.Add(new Grade { Subject = "Math", Value = 5, Date = DateTime.UtcNow.AddDays(-1) });
        student1.Grades.Add(new Grade { Subject = "History", Value = 4, Date = DateTime.UtcNow });

        var student2 = new Student { FullName = "Kirill Illin", Class = "6B" };
        student2.Grades.Add(new Grade { Subject = "Math", Value = 3, Date = DateTime.UtcNow });
        student2.Grades.Add(new Grade { Subject = "English", Value = 5, Date = DateTime.UtcNow });

        context.Students.AddRange(student1, student2);
        context.SaveChanges();
    }
}
