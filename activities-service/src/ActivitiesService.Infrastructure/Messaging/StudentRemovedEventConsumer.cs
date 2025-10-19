using MassTransit;
using School.Contracts;
using ActivitiesService.Application.Ports;

namespace ActivitiesService.Infrastructure.Messaging;

public class StudentRemovedEventConsumer : IConsumer<StudentRemoved>
{
    private readonly IEnrollmentService _svc;
    public StudentRemovedEventConsumer(IEnrollmentService svc) => _svc = svc;

    public async Task Consume(ConsumeContext<StudentRemoved> context)
    {
        await _svc.CancelAllForStudentAsync(context.Message.StudentId, context.CancellationToken);
    }
}
