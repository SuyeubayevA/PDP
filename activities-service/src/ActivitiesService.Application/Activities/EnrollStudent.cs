using MediatR;
using ActivitiesService.Domain.Entities;
using ActivitiesService.Domain.Repositories;

namespace ActivitiesService.Application.Activities;

public record EnrollStudentCommand(int ActivityId, int StudentId) : IRequest<int>;

public class EnrollStudentHandler : IRequestHandler<EnrollStudentCommand, int>
{
    private readonly IActivitiesRepository _repo;
    public EnrollStudentHandler(IActivitiesRepository repo) => _repo = repo;

    public async Task<int> Handle(EnrollStudentCommand request, CancellationToken ct)
    {
        var activity = await _repo.GetActivityAsync(request.ActivityId, ct);
        if (activity is null || !activity.IsActive) throw new InvalidOperationException("Activity not available");

        var current = await _repo.CountActiveEnrollmentsAsync(request.ActivityId, ct);
        if (current >= activity.Capacity) throw new InvalidOperationException("No seats");

        var enroll = new ActivityEnrollment
        {
            ActivityId = request.ActivityId,
            StudentId = request.StudentId,
            Status = EnrollmentStatus.Enrolled,
            EnrolledUtc = DateTime.UtcNow
        };
        await _repo.AddEnrollmentAsync(enroll, ct);
        await _repo.SaveChangesAsync(ct);
        return enroll.Id;
    }
}
