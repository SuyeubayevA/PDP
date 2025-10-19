using MediatR;
using StudentDiary.Application.DTOs;
using StudentDiary.Domain.Repositories;

namespace StudentDiary.Application.Students;

public record GetStudentQuery(int Id) : IRequest<StudentDto?>;

public class GetStudentHandler : IRequestHandler<GetStudentQuery, StudentDto?>
{
    private readonly IStudentRepository _repo;
    public GetStudentHandler(IStudentRepository repo) => _repo = repo;

    public async Task<StudentDto?> Handle(GetStudentQuery request, CancellationToken cancellationToken)
    {
        var s = await _repo.GetByIdAsync(request.Id, cancellationToken);
        if (s == null) return null;

        return new StudentDto
        {
            Id = s.Id,
            FullName = s.FullName,
            Class = s.Class,
            IsActive = s.IsActive,
            Grades = s.Grades.Select(g => new GradeDto
            {
                Id = g.Id,
                Subject = g.Subject,
                Value = g.Value,
                Date = g.Date
            }).ToList()
        };
    }
}
