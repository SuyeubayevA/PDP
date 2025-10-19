using MassTransit;
using Microsoft.EntityFrameworkCore;
using Polly;
using Serilog;
using Serilog.Events;
using StudentDiary.Application.Ports;
using StudentDiary.Application.Students;
using StudentDiary.Domain.Repositories;
using StudentDiary.Infrastructure.Data;
using StudentDiary.Infrastructure.HttpPolicies;
using StudentDiary.Infrastructure.Integrations;
using StudentDiary.Infrastructure.Outbox;
using StudentDiary.Infrastructure.Repositories;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/app.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddDbContext<StudentDiaryDbContext>(opt => opt.UseInMemoryDatabase("StudentDiaryDb"));

builder.Services.AddSingleton<PaymentsApiPolicy>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<OutboxRepository>();
builder.Services.AddHostedService<OutboxProcessor>();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateStudentCommand).Assembly));

var paymentBase = builder.Configuration["PAYMENTS__BASEURL"] ?? "http://localhost:5063/";
builder.Services
    .AddHttpClient<IPaymentsService, PaymentsApiClient>(c =>
    {
        c.BaseAddress = new Uri(paymentBase); // payment-service
    })
    .AddPolicyHandler((sp, request) =>
    {
        var policy = sp.GetRequiredService<PaymentsApiPolicy>();
        return policy.GetPolicy();
    });

var rabbitHost = builder.Configuration["RABBITMQ__HOST"] ?? "rabbitmq";
var rabbitUser = builder.Configuration["RABBITMQ__USER"] ?? "admin";
var rabbitPass = builder.Configuration["RABBITMQ__PASS"] ?? "HPSRabbitQ";

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitHost, "/", h =>
        {
            h.Username(rabbitUser);
            h.Password(rabbitPass);
        });
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<StudentDiaryDbContext>();
    DbInitializer.Seed(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
