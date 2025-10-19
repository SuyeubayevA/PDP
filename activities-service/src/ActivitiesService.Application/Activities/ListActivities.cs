using MediatR;
using ActivitiesService.Domain.Entities;
using ActivitiesService.Domain.Repositories;

namespace ActivitiesService.Application.Activities;

public record ListActivitiesQuery() : IRequest<List<Activity>>;

public class ListActivitiesHandler : IRequestHandler<ListActivitiesQuery, List<Activity>>
{
    private readonly IActivitiesRepository _repo;
    public ListActivitiesHandler(IActivitiesRepository repo) => _repo = repo;

    public Task<List<Activity>> Handle(ListActivitiesQuery request, CancellationToken ct) =>
        _repo.GetAllActivitiesAsync(ct);
}
