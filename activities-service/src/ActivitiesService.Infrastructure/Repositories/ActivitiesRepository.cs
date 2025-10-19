using Microsoft.EntityFrameworkCore;
using ActivitiesService.Domain.Entities;
using ActivitiesService.Domain.Repositories;
using ActivitiesService.Infrastructure.Data;

namespace ActivitiesService.Infrastructure.Repositories;

public class ActivitiesRepository : IActivitiesRepository
{
    private readonly ActivitiesDbContext _db;
    public ActivitiesRepository(ActivitiesDbContext db) => _db = db;

    public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);

    public async Task AddActivityAsync(Activity activity, CancellationToken ct = default)
    { _db.Activities.Add(activity); await Task.CompletedTask; }

    public Task<Activity?> GetActivityAsync(int activityId, CancellationToken ct = default) =>
        _db.Activities.FirstOrDefaultAsync(a => a.Id == activityId, ct);

    public Task<List<Activity>> GetAllActivitiesAsync(CancellationToken ct = default) =>
    _db.Activities.ToListAsync(ct);

    public Task<int> CountActiveEnrollmentsAsync(int activityId, CancellationToken ct = default) =>
        _db.Enrollments.CountAsync(e => e.ActivityId == activityId && e.Status == EnrollmentStatus.Enrolled, ct);

    public async Task AddEnrollmentAsync(ActivityEnrollment enrollment, CancellationToken ct = default)
    { _db.Enrollments.Add(enrollment); await Task.CompletedTask; }

    public Task<List<ActivityEnrollment>> ListEnrollmentsAsync(int activityId, CancellationToken ct = default) =>
        _db.Enrollments.Where(e => e.ActivityId == activityId).OrderByDescending(e => e.EnrolledUtc).ToListAsync(ct);

    public Task<List<ActivityEnrollment>> ListEnrollmentsByStudentAsync(int studentId, CancellationToken ct = default) =>
        _db.Enrollments.Where(e => e.StudentId == studentId).ToListAsync(ct);
}
