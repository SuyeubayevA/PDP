using MassTransit;
using School.Contracts;
using ActivitiesService.Domain.Entities;
using ActivitiesService.Domain.Repositories;

namespace ActivitiesService.Infrastructure.Messaging;

public class StudentEnrolledConsumer : IConsumer<StudentEnrolledInActivity>
{
    private readonly IActivitiesRepository _repo;

    public StudentEnrolledConsumer(IActivitiesRepository repo) => _repo = repo;

    public async Task Consume(ConsumeContext<StudentEnrolledInActivity> context)
    {
        var msg = context.Message;

        var activity = await _repo.GetActivityAsync(msg.ActivityId, context.CancellationToken);
        if (activity == null || !activity.IsActive) return;

        var enroll = new ActivityEnrollment
        {
            ActivityId = msg.ActivityId,
            StudentId = msg.StudentId,
            Status = EnrollmentStatus.Enrolled,
            EnrolledUtc = DateTime.UtcNow
        };

        await _repo.AddEnrollmentAsync(enroll, context.CancellationToken);
        await _repo.SaveChangesAsync(context.CancellationToken);
    }
}
