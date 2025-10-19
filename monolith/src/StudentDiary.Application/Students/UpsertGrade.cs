using MediatR;
using StudentDiary.Domain.Entities;
using StudentDiary.Domain.Repositories;

namespace StudentDiary.Application.Students;

public record UpsertGradeCommand(int StudentId, string Subject, int Value) : IRequest;

public class UpsertGradeHandler : IRequestHandler<UpsertGradeCommand>
{
    private readonly IStudentRepository _repo;
    public UpsertGradeHandler(IStudentRepository repo) => _repo = repo;

    public async Task Handle(UpsertGradeCommand request, CancellationToken cancellationToken)
    {
        if (request.Value < 1 || request.Value > 5)
            throw new ArgumentException("Grade must be between 1 and 5");

        var student = await _repo.GetByIdAsync(request.StudentId, cancellationToken);
        if (student == null)
            throw new Exception("Student not found");

        var today = DateTime.UtcNow.Date;

        var existingGrade = student.Grades
            .FirstOrDefault(g => g.Subject.Equals(request.Subject, StringComparison.OrdinalIgnoreCase)
                              && g.Date.Date == today);

        if (existingGrade is null)
        {
            var grade = new Grade
            {
                StudentId = student.Id,
                Subject = request.Subject,
                Value = request.Value,
                Date = DateTime.UtcNow
            };

            _repo.AddGrade(grade);
        }
        else
        {
            existingGrade.Value = request.Value;
        }

        await _repo.UpdateAsync(student, cancellationToken);
    }
}
