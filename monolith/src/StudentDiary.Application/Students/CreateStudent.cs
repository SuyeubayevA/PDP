using MediatR;
using StudentDiary.Domain.Entities;
using StudentDiary.Domain.Repositories;

namespace StudentDiary.Application.Students;

public record CreateStudentCommand(string FullName, string Class) : IRequest<int>;

public class CreateStudentHandler : IRequestHandler<CreateStudentCommand, int>
{
    private readonly IStudentRepository _repo;
    public CreateStudentHandler(IStudentRepository repo) => _repo = repo;

    public async Task<int> Handle(CreateStudentCommand request, CancellationToken cancellationToken)
    {
        var student = new Student
        {
            FullName = request.FullName,
            Class = request.Class
        };

        await _repo.AddAsync(student, cancellationToken);
        return student.Id;
    }
}
