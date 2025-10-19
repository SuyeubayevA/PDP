namespace ActivitiesService.Application.Ports;

public interface IEnrollmentService
{
    Task CancelAllForStudentAsync(int studentId, CancellationToken ct = default);
}