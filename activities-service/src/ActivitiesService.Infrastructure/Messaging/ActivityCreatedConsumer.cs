using MassTransit;
using School.Contracts;
using ActivitiesService.Domain.Entities;
using ActivitiesService.Domain.Repositories;

namespace ActivitiesService.Infrastructure.Messaging;

public class ActivityCreatedConsumer : IConsumer<ActivityCreated>
{
    private readonly IActivitiesRepository _repo;

    public ActivityCreatedConsumer(IActivitiesRepository repo) => _repo = repo;

    public async Task Consume(ConsumeContext<ActivityCreated> context)
    {
        var msg = context.Message;

        var activity = new Activity
        {
            Name = msg.Name,
            Schedule = msg.Schedule,
            Capacity = msg.Capacity,
            IsActive = true
        };

        await _repo.AddActivityAsync(activity, context.CancellationToken);
        await _repo.SaveChangesAsync(context.CancellationToken);
    }
}
