using Microsoft.EntityFrameworkCore;
using StudentDiary.Domain.Entities;
using StudentDiary.Domain.Repositories;
using StudentDiary.Infrastructure.Data;

namespace StudentDiary.Infrastructure.Repositories;

public class StudentRepository : IStudentRepository
{
    private readonly StudentDiaryDbContext _db;
    public StudentRepository(StudentDiaryDbContext db) => _db = db;

    public async Task AddAsync(Student student, CancellationToken ct = default)
    {
        _db.Students.Add(student);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<Student?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await _db.Students.Include(s => s.Grades).FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<IEnumerable<Student>> ListAsync(CancellationToken ct = default) =>
        await _db.Students.Include(s => s.Grades).ToListAsync(ct);

    public async Task UpdateAsync(Student student, CancellationToken ct = default)
    {
        //_db.Students.Update(student);
        await _db.SaveChangesAsync(ct);
    }

    public void AddGrade(Grade grade)
    {
        _db.Grades.Add(grade);
    }

    public async Task DeleteAsync(Student student, CancellationToken ct = default)
    {
        _db.Students.Remove(student);
        await _db.SaveChangesAsync(ct);
    }
}
