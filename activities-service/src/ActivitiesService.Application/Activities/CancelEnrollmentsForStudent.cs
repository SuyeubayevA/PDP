using ActivitiesService.Application.Ports;
using ActivitiesService.Domain.Entities;
using ActivitiesService.Domain.Repositories;

namespace ActivitiesService.Application.Activities;

public class EnrollmentService : IEnrollmentService
{
    private readonly IActivitiesRepository _repo;
    public EnrollmentService(IActivitiesRepository repo) => _repo = repo;

    public async Task CancelAllForStudentAsync(int studentId, CancellationToken ct = default)
    {
        var items = await _repo.ListEnrollmentsByStudentAsync(studentId, ct);
        foreach (var e in items.Where(x => x.Status == EnrollmentStatus.Enrolled))
            e.Status = EnrollmentStatus.Cancelled;

        await _repo.SaveChangesAsync(ct);
    }
}
