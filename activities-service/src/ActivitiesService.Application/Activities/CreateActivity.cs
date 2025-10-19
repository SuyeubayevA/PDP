using MediatR;
using ActivitiesService.Domain.Entities;
using ActivitiesService.Domain.Repositories;

namespace ActivitiesService.Application.Activities;

public record CreateActivityCommand(string Name, string Schedule, int Capacity) : IRequest<int>;

public class CreateActivityHandler : IRequestHandler<CreateActivityCommand, int>
{
    private readonly IActivitiesRepository _repo;
    public CreateActivityHandler(IActivitiesRepository repo) => _repo = repo;

    public async Task<int> Handle(CreateActivityCommand request, CancellationToken ct)
    {
        var a = new Activity { Name = request.Name, Schedule = request.Schedule, Capacity = request.Capacity, IsActive = true };
        await _repo.AddActivityAsync(a, ct);
        await _repo.SaveChangesAsync(ct);
        return a.Id;
    }
}
