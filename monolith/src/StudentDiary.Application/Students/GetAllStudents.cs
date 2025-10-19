using MediatR;
using StudentDiary.Application.DTOs;
using StudentDiary.Domain.Repositories;

namespace StudentDiary.Application.Students;

public record GetAllStudentsQuery() : IRequest<List<StudentDto>>;

public class GetAllStudentsHandler : IRequestHandler<GetAllStudentsQuery, List<StudentDto>>
{
    private readonly IStudentRepository _repo;
    public GetAllStudentsHandler(IStudentRepository repo) => _repo = repo;

    public async Task<List<StudentDto>> Handle(GetAllStudentsQuery request, CancellationToken cancellationToken)
    {
        var students = await _repo.ListAsync(cancellationToken);

        return students.Select(s => new StudentDto
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
        }).ToList();
    }
}
