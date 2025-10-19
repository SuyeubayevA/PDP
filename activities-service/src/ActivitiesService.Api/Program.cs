using ActivitiesService.Application.Activities;
using ActivitiesService.Application.Ports;
using ActivitiesService.Domain.Repositories;
using ActivitiesService.Infrastructure.Data;
using ActivitiesService.Infrastructure.Messaging;
using ActivitiesService.Infrastructure.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using School.Contracts;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// EF InMemory
builder.Services.AddDbContext<ActivitiesDbContext>(opt => opt.UseInMemoryDatabase("ActivitiesDb"));

// DI
builder.Services.AddScoped<IActivitiesRepository, ActivitiesRepository>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateActivityCommand).Assembly));

// MassTransit + RabbitMQ
var rabbitHost = builder.Configuration["RABBITMQ__HOST"] ?? "rabbitmq";
var rabbitUser = builder.Configuration["RABBITMQ__USER"] ?? "guest";
var rabbitPass = builder.Configuration["RABBITMQ__PASS"] ?? "guest";

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<StudentRemovedEventConsumer>();
    x.AddConsumer<ActivityCreatedConsumer>();
    x.AddConsumer<StudentEnrolledConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitHost, "/", h =>
        {
            h.Username(rabbitUser);
            h.Password(rabbitPass);
        });

        cfg.ReceiveEndpoint("activities.student-removed", e =>
        {
            e.ConfigureConsumer<StudentRemovedEventConsumer>(context);

            e.UseMessageRetry(r =>
            {
                r.Interval(3, TimeSpan.FromSeconds(5));
                r.Handle<InvalidOperationException>();
                r.Handle<DbUpdateException>();
            });

            e.SetQueueArgument("x-dead-letter-exchange", "activities.dlx");
            e.SetQueueArgument("x-dead-letter-routing-key", "activities.student-removed.dlq");
        });

        cfg.ReceiveEndpoint("activities.activity-created", e =>
        {
            e.ConfigureConsumer<ActivityCreatedConsumer>(context);

            e.UseMessageRetry(r =>
            {
                r.Interval(5, TimeSpan.FromSeconds(2));
                r.Handle<DbUpdateException>();
                r.Handle<InvalidOperationException>();
                r.Ignore<ArgumentException>();
                r.Ignore<ArgumentNullException>();
            });

            e.SetQueueArgument("x-dead-letter-exchange", "activities.dlx");
            e.SetQueueArgument("x-dead-letter-routing-key", "activities.activity-created.dlq");
        });

        cfg.ReceiveEndpoint("activities.student-enrolled", e =>
        {
            e.ConfigureConsumer<StudentEnrolledConsumer>(context);

            e.UseMessageRetry(r =>
            {
                r.Interval(5, TimeSpan.FromSeconds(1));
                r.Handle<DbUpdateException>();
                r.Handle<InvalidOperationException>();
                r.Ignore<ArgumentException>();
                r.Ignore<ArgumentNullException>();
            });

            e.SetQueueArgument("x-dead-letter-exchange", "activities.dlx");
            e.SetQueueArgument("x-dead-letter-routing-key", "activities.student-enrolled.dlq");
        });

        cfg.ReceiveEndpoint("activities.student-removed.dlq", dlq =>
        {
            dlq.Handler<StudentRemoved>(async ctx =>
            {
                Console.WriteLine($"[DLQ] student-removed StudentId={ctx.Message.StudentId}");
            });
        });

        cfg.ReceiveEndpoint("activities.activity-created.dlq", dlq =>
        {
            dlq.Handler<ActivityCreated>(async ctx =>
            {
                Console.WriteLine($"[DLQ] activity-created Name={ctx.Message.Name} Capacity={ctx.Message.Capacity}");
            });
        });

        cfg.ReceiveEndpoint("activities.student-enrolled.dlq", dlq =>
        {
            dlq.Handler<StudentEnrolledInActivity>(async ctx =>
            {
                Console.WriteLine($"[DLQ] student-enrolled ActivityId={ctx.Message.ActivityId} StudentId={ctx.Message.StudentId}");
            });
        });
    });
});

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); app.UseSwaggerUI();
}

app.MapControllers();
app.Run();
