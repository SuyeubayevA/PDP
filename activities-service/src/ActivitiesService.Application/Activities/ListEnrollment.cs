using MediatR;
using ActivitiesService.Domain.Entities;
using ActivitiesService.Domain.Repositories;

namespace ActivitiesService.Application.Activities;

public record ListEnrollmentsQuery(int ActivityId) : IRequest<List<ActivityEnrollment>>;

public class ListEnrollmentsHandler : IRequestHandler<ListEnrollmentsQuery, List<ActivityEnrollment>>
{
    private readonly IActivitiesRepository _repo;
    public ListEnrollmentsHandler(IActivitiesRepository repo) => _repo = repo;

    public Task<List<ActivityEnrollment>> Handle(ListEnrollmentsQuery request, CancellationToken ct) =>
        _repo.ListEnrollmentsAsync(request.ActivityId, ct);
}
