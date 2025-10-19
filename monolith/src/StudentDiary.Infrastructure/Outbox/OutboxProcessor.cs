using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StudentDiary.Infrastructure.Repositories;
using System.Text.Json;

namespace StudentDiary.Infrastructure.Outbox;

public class OutboxProcessor : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<OutboxProcessor> _logger;

    public OutboxProcessor(IServiceProvider services, ILogger<OutboxProcessor> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _services.CreateScope();

            var repo = scope.ServiceProvider.GetRequiredService<OutboxRepository>();
            var bus = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

            var messages = await repo.GetUnprocessedAsync(stoppingToken);

            bool anyProcessed = false;

            foreach (var msg in messages)
            {
                try
                {
                    var type = Type.GetType($"School.Contracts.{msg.Type}, School.Contracts");
                    if (type == null)
                    {
                        _logger.LogError("Unknown event type {Type}", msg.Type);
                        continue;
                    }

                    var evt = JsonSerializer.Deserialize(msg.Payload, type);
                    if (evt != null)
                    {
                        await bus.Publish(evt, stoppingToken);
                        msg.ProcessedUtc = DateTime.UtcNow;
                        anyProcessed = true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing OutboxMessage {Id}", msg.Id);
                }
            }

            if (anyProcessed)
            {
                await repo.SaveChangesAsync(stoppingToken);
            }

            await Task.Delay(2000, stoppingToken);
        }
    }
}
