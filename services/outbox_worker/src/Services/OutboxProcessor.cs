using Microsoft.EntityFrameworkCore;
using OutboxWorker.Infrastructure;
using Serilog;
using System.Diagnostics;

namespace OutboxWorker.Services
{
    public class OutboxProcessor : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OutboxProcessor> _logger;
        private readonly RabbitMqPublisher _rabbitMqPublisher;
        private readonly TimeSpan _processingInterval = TimeSpan.FromSeconds(5); // Poll every 5 seconds
        private const int MaxRetries = 5;
        private static readonly ActivitySource ActivitySource = new ActivitySource("OutboxWorker");

        public OutboxProcessor(IServiceProvider serviceProvider, ILogger<OutboxProcessor> logger, RabbitMqPublisher rabbitMqPublisher)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _rabbitMqPublisher = rabbitMqPublisher;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Outbox Processor started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                using (var activity = ActivitySource.StartActivity("ProcessOutboxMessages"))
                {
                    try
                    {
                        await ProcessOutboxMessagesAsync(stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error occurred while processing outbox messages.");
                        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    }
                }
                await Task.Delay(_processingInterval, stoppingToken);
            }

            _logger.LogInformation("Outbox Processor stopped.");
        }

        private async Task ProcessOutboxMessagesAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OutboxDbContext>();

            var messages = await dbContext.OutboxMessages
                .Where(m => m.ProcessedDate == null && m.RetryCount < MaxRetries)
                .OrderBy(m => m.OccurredOn)
                .Take(100) // Process a batch of messages
                .ToListAsync(stoppingToken);

            if (!messages.Any())
            {
                _logger.LogDebug("No pending outbox messages found.");
                return;
            }

            _logger.LogInformation("Found {Count} pending outbox messages to process.", messages.Count);

            foreach (var message in messages)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Cancellation requested, stopping message processing.");
                    return;
                }

                using (var messageActivity = ActivitySource.StartActivity("PublishOutboxMessage", ActivityKind.Producer))
                {
                    messageActivity?.SetTag("message.id", message.Id);
                    messageActivity?.SetTag("message.type", message.Type);

                    try
                    {
                        // Assuming message.Type can be used as routing key and exchange
                        // In a real scenario, you might have a more sophisticated mapping
                        await _rabbitMqPublisher.PublishMessage("nova_commerce_exchange", message.Type, message.Data);

                        message.ProcessedDate = DateTime.UtcNow;
                        message.Error = null;
                        _logger.LogInformation("Outbox message {MessageId} of type {MessageType} processed successfully.", message.Id, message.Type);
                    }
                    catch (Exception ex)
                    {
                        message.RetryCount++;
                        message.Error = ex.Message;
                        _logger.LogError(ex, "Failed to process outbox message {MessageId} of type {MessageType}. Retry count: {RetryCount}", message.Id, message.Type, message.RetryCount);
                        messageActivity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    }
                }
            }
            await dbContext.SaveChangesAsync(stoppingToken);
        }
    }
}
