using StudentDiary.Domain.Entities;

namespace StudentDiary.Domain.Repositories;

public interface IStudentRepository
{
    Task<Student?> GetByIdAsync(int id, CancellationToken ct = default);
    Task AddAsync(Student student, CancellationToken ct = default);
    Task UpdateAsync(Student student, CancellationToken ct = default);
    Task<IEnumerable<Student>> ListAsync(CancellationToken ct = default);
    void AddGrade(Grade grade);
    Task DeleteAsync(Student student, CancellationToken ct = default);
}
