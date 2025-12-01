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

namespace OutboxWorker.UnitTests
{
    public class OutboxProcessorTests
    {
        private readonly Mock<ILogger<OutboxProcessor>> _mockLogger;
        private readonly Mock<RabbitMqPublisher> _mockRabbitMqPublisher;
        private readonly ServiceProvider _serviceProvider;

        public OutboxProcessorTests()
        {
            _mockLogger = new Mock<ILogger<OutboxProcessor>>();
            _mockRabbitMqPublisher = new Mock<RabbitMqPublisher>("amqp://guest:guest@localhost:5672"); // Dummy connection string

            var services = new ServiceCollection();
            services.AddDbContext<OutboxDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestOutboxDb");
            });
            services.AddSingleton(_mockRabbitMqPublisher.Object);
            services.AddSingleton(_mockLogger.Object);
            _serviceProvider = services.BuildServiceProvider();
        }

        private OutboxDbContext GetDbContext()
        {
            var dbContext = _serviceProvider.GetRequiredService<OutboxDbContext>();
            dbContext.Database.EnsureDeleted(); // Clear the database before each test
            dbContext.Database.EnsureCreated();
            return dbContext;
        }

        [Fact]
        public async Task ProcessOutboxMessagesAsync_PublishesMessagesAndMarksAsProcessed()
        {
            // Arrange
            var dbContext = GetDbContext();
            var message1 = new OutboxMessage { Id = Guid.NewGuid(), OccurredOn = DateTime.UtcNow, Type = "TestEvent1", Data = "{\"Key\":\"Value1\"}" };
            var message2 = new OutboxMessage { Id = Guid.NewGuid(), OccurredOn = DateTime.UtcNow, Type = "TestEvent2", Data = "{\"Key\":\"Value2\"}" };
            await dbContext.OutboxMessages.AddRangeAsync(message1, message2);
            await dbContext.SaveChangesAsync();

            var outboxProcessor = new OutboxProcessor(_serviceProvider, _mockLogger.Object, _mockRabbitMqPublisher.Object);
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(1)); // Run for a short period

            // Act
            await outboxProcessor.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(TimeSpan.FromMilliseconds(100)); // Give some time for the background service to run
            await outboxProcessor.StopAsync(cancellationTokenSource.Token);

            // Assert
            _mockRabbitMqPublisher.Verify(p => p.PublishMessage("nova_commerce_exchange", "TestEvent1", "{\"Key\":\"Value1\"}"), Times.Once);
            _mockRabbitMqPublisher.Verify(p => p.PublishMessage("nova_commerce_exchange", "TestEvent2", "{\"Key\":\"Value2\"}"), Times.Once);

            var processedMessage1 = await dbContext.OutboxMessages.FindAsync(message1.Id);
            var processedMessage2 = await dbContext.OutboxMessages.FindAsync(message2.Id);

            Assert.NotNull(processedMessage1.ProcessedDate);
            Assert.NotNull(processedMessage2.ProcessedDate);
            Assert.Null(processedMessage1.Error);
            Assert.Null(processedMessage2.Error);
        }

        [Fact]
        public async Task ProcessOutboxMessagesAsync_HandlesPublishingErrorsAndRetries()
        {
            // Arrange
            var dbContext = GetDbContext();
            var message = new OutboxMessage { Id = Guid.NewGuid(), OccurredOn = DateTime.UtcNow, Type = "TestEventError", Data = "{\"Key\":\"ValueError\"}" };
            await dbContext.OutboxMessages.AddAsync(message);
            await dbContext.SaveChangesAsync();

            _mockRabbitMqPublisher.Setup(p => p.PublishMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("RabbitMQ connection error"));

            var outboxProcessor = new OutboxProcessor(_serviceProvider, _mockLogger.Object, _mockRabbitMqPublisher.Object);
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(1));

            // Act
            await outboxProcessor.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            await outboxProcessor.StopAsync(cancellationTokenSource.Token);

            // Assert
            _mockRabbitMqPublisher.Verify(p => p.PublishMessage("nova_commerce_exchange", "TestEventError", "{\"Key\":\"ValueError\"}"), Times.AtLeastOnce);

            var failedMessage = await dbContext.OutboxMessages.FindAsync(message.Id);
            Assert.Null(failedMessage.ProcessedDate);
            Assert.Equal(1, failedMessage.RetryCount);
            Assert.Contains("RabbitMQ connection error", failedMessage.Error);
        }

        [Fact]
        public async Task ProcessOutboxMessagesAsync_DoesNotProcessAlreadyProcessedMessages()
        {
            // Arrange
            var dbContext = GetDbContext();
            var message = new OutboxMessage { Id = Guid.NewGuid(), OccurredOn = DateTime.UtcNow, Type = "TestEventProcessed", Data = "{\"Key\":\"ValueProcessed\"}", ProcessedDate = DateTime.UtcNow };
            await dbContext.OutboxMessages.AddAsync(message);
            await dbContext.SaveChangesAsync();

            var outboxProcessor = new OutboxProcessor(_serviceProvider, _mockLogger.Object, _mockRabbitMqPublisher.Object);
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(1));

            // Act
            await outboxProcessor.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            await outboxProcessor.StopAsync(cancellationTokenSource.Token);

            // Assert
            _mockRabbitMqPublisher.Verify(p => p.PublishMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
    }
}
