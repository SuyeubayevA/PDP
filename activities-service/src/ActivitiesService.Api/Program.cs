using ActivitiesService.Application.Activities;
using ActivitiesService.Application.Ports;
using ActivitiesService.Domain.Repositories;
using ActivitiesService.Infrastructure.Data;
using ActivitiesService.Infrastructure.Messaging;
using ActivitiesService.Infrastructure.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;

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
var rabbitHost = builder.Configuration["RABBITMQ__HOST"] ?? "guest";
var rabbitUser = builder.Configuration["RABBITMQ__USER"] ?? "guest";
var rabbitPass = builder.Configuration["RABBITMQ__PASS"] ?? "Your_str0ng_P@ssw0rd";

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

            e.DiscardFaultedMessages();
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

            e.DiscardFaultedMessages();
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

            e.DiscardFaultedMessages();
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
