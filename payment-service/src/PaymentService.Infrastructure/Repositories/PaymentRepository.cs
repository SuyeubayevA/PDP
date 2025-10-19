using Microsoft.EntityFrameworkCore;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Repositories;
using PaymentService.Infrastructure.Data;

namespace PaymentService.Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly PaymentsDbContext _db;
    public PaymentRepository(PaymentsDbContext db) => _db = db;

    public async Task AddAsync(Payment payment, CancellationToken ct = default)
    {
        _db.Payments.Add(payment);
        await Task.CompletedTask;
    }

    public async Task<Payment?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await _db.Payments.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<Payment>> ListByStudentAsync(int studentId, CancellationToken ct = default) =>
        await _db.Payments.Where(p => p.StudentId == studentId)
                          .OrderByDescending(p => p.CreatedUtc)
                          .ToListAsync(ct);

    public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
