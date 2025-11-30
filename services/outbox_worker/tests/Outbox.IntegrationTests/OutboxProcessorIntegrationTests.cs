using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using OutboxWorker.Services;
using OutboxWorker.Infrastructure;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace OutboxWorker.IntegrationTests
{
    public class OutboxProcessorIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly Mock<RabbitMqPublisher> _mockRabbitMqPublisher;

        public OutboxProcessorIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _mockRabbitMqPublisher = new Mock<RabbitMqPublisher>("amqp://guest:guest@localhost:5672"); // Dummy connection string
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the existing DbContext registration
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<OutboxDbContext>));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // Add DbContextOptions for in-memory database
                    services.AddDbContext<OutboxDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("IntegrationTestOutboxDb");
                    });

                    // Replace RabbitMqPublisher with our mock
                    var rabbitMqPublisherDescriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(RabbitMqPublisher));
                    if (rabbitMqPublisherDescriptor != null)
                    {
                        services.Remove(rabbitMqPublisherDescriptor);
                    }
                    services.AddSingleton(_mockRabbitMqPublisher.Object);
                });
            });
        }

        private OutboxDbContext GetDbContext(IServiceScope scope)
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<OutboxDbContext>();
            dbContext.Database.EnsureDeleted(); // Clear the database before each test
            dbContext.Database.EnsureCreated();
            return dbContext;
        }

        [Fact]
        public async Task OutboxProcessor_PublishesMessagesAndMarksAsProcessed()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var dbContext = GetDbContext(scope);

            var message1 = new OutboxMessage { Id = Guid.NewGuid(), OccurredOn = DateTime.UtcNow, Type = "IntegrationTestEvent1", Data = "{\"Key\":\"Value1\"}" };
            var message2 = new OutboxMessage { Id = Guid.NewGuid(), OccurredOn = DateTime.UtcNow, Type = "IntegrationTestEvent2", Data = "{\"Key\":\"Value2\"}" };
            await dbContext.OutboxMessages.AddRangeAsync(message1, message2);
            await dbContext.SaveChangesAsync();

            var host = _factory.Services.GetRequiredService<IHostedService>() as OutboxProcessor;
            Assert.NotNull(host);

            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5)); // Allow some time for processing

            // Act
            await host.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(TimeSpan.FromSeconds(1)); // Give the background service some time to run
            await host.StopAsync(cancellationTokenSource.Token);

            // Assert
            _mockRabbitMqPublisher.Verify(p => p.PublishMessage("nova_commerce_exchange", "IntegrationTestEvent1", "{\"Key\":\"Value1\"}"), Times.Once);
            _mockRabbitMqPublisher.Verify(p => p.PublishMessage("nova_commerce_exchange", "IntegrationTestEvent2", "{\"Key\":\"Value2\"}"), Times.Once);

            var processedMessage1 = await dbContext.OutboxMessages.FindAsync(message1.Id);
            var processedMessage2 = await dbContext.OutboxMessages.FindAsync(message2.Id);

            Assert.NotNull(processedMessage1.ProcessedDate);
            Assert.NotNull(processedMessage2.ProcessedDate);
            Assert.Null(processedMessage1.Error);
            Assert.Null(processedMessage2.Error);
        }

        [Fact]
        public async Task OutboxProcessor_HandlesPublishingErrorsAndRetries()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var dbContext = GetDbContext(scope);

            var message = new OutboxMessage { Id = Guid.NewGuid(), OccurredOn = DateTime.UtcNow, Type = "IntegrationTestErrorEvent", Data = "{\"Key\":\"ValueError\"}" };
            await dbContext.OutboxMessages.AddAsync(message);
            await dbContext.SaveChangesAsync();

            _mockRabbitMqPublisher.Setup(p => p.PublishMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Simulated RabbitMQ connection error"));

            var host = _factory.Services.GetRequiredService<IHostedService>() as OutboxProcessor;
            Assert.NotNull(host);

            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            // Act
            await host.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(TimeSpan.FromSeconds(1));
            await host.StopAsync(cancellationTokenSource.Token);

            // Assert
            _mockRabbitMqPublisher.Verify(p => p.PublishMessage("nova_commerce_exchange", "IntegrationTestErrorEvent", "{\"Key\":\"ValueError\"}"), Times.AtLeastOnce);

            var failedMessage = await dbContext.OutboxMessages.FindAsync(message.Id);
            Assert.Null(failedMessage.ProcessedDate);
            Assert.Equal(1, failedMessage.RetryCount);
            Assert.Contains("Simulated RabbitMQ connection error", failedMessage.Error);
        }
    }
}
