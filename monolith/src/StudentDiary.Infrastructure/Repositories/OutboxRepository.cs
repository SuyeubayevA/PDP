using StudentDiary.Domain.Entities;
using StudentDiary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace StudentDiary.Infrastructure.Repositories;

public class OutboxRepository
{
    private readonly StudentDiaryDbContext _db;
    public OutboxRepository(StudentDiaryDbContext db) => _db = db;

    public Task AddAsync(OutboxMessage message, CancellationToken ct = default)
    {
        _db.OutboxMessages.Add(message);
        return Task.CompletedTask;
    }

    public async Task<List<OutboxMessage>> GetUnprocessedAsync(CancellationToken ct = default)
    {
        return await _db.OutboxMessages
            .Where(m => m.ProcessedUtc == null)
            .OrderBy(m => m.CreatedUtc)
            .Take(50)
            .ToListAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
