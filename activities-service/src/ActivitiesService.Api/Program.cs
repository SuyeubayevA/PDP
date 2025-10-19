using MediatR;
using Microsoft.EntityFrameworkCore;
using MassTransit;
using ActivitiesService.Application.Activities;
using ActivitiesService.Application.Ports;
using ActivitiesService.Domain.Repositories;
using ActivitiesService.Infrastructure.Data;
using ActivitiesService.Infrastructure.Repositories;
using ActivitiesService.Infrastructure.Messaging;

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
        });

        cfg.ReceiveEndpoint("activities.activity-created", e =>
        {
            e.ConfigureConsumer<ActivityCreatedConsumer>(context);
        });

        cfg.ReceiveEndpoint("activities.student-enrolled", e =>
        {
            e.ConfigureConsumer<StudentEnrolledConsumer>(context);
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
