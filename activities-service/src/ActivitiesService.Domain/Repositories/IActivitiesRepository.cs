using ActivitiesService.Domain.Entities;

namespace ActivitiesService.Domain.Repositories;

public interface IActivitiesRepository
{
    Task AddActivityAsync(Activity activity, CancellationToken ct = default);
    Task<Activity?> GetActivityAsync(int activityId, CancellationToken ct = default);
    Task<List<Activity>> GetAllActivitiesAsync(CancellationToken ct = default);

    Task<int> CountActiveEnrollmentsAsync(int activityId, CancellationToken ct = default);
    Task AddEnrollmentAsync(ActivityEnrollment enrollment, CancellationToken ct = default);
    Task<List<ActivityEnrollment>> ListEnrollmentsAsync(int activityId, CancellationToken ct = default);
    Task<List<ActivityEnrollment>> ListEnrollmentsByStudentAsync(int studentId, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
