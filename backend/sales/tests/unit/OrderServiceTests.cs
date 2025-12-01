using System;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using NovaCommerce.Sales.Application;
using NovaCommerce.Sales.Domain;
using NovaCommerce.Sales.Infrastructure.Messaging;
using Xunit;

namespace NovaCommerce.Sales.UnitTests
{
    public class OrderServiceTests
    {
        private readonly Mock<IOrderRepository> _orderRepositoryMock;
        private readonly Mock<HttpClient> _httpClientMock;
        private readonly Mock<MessagePublisher> _messagePublisherMock;
        private readonly OrderService _orderService;

        public OrderServiceTests()
        {
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _httpClientMock = new Mock<HttpClient>();
            _messagePublisherMock = new Mock<MessagePublisher>("amqp://guest:guest@localhost:5672/");
            _orderService = new OrderService(_orderRepositoryMock.Object, _httpClientMock.Object, _messagePublisherMock.Object);
        }

        [Fact]
        public async Task GetOrderById_Should_Return_Order()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var order = new Order { OrderId = orderId, CustomerId = Guid.NewGuid() };
            _orderRepositoryMock.Setup(repo => repo.GetByIdAsync(orderId)).ReturnsAsync(order);

            // Act
            var result = await _orderService.GetOrderById(orderId);

            // Assert
            Assert.Equal(orderId, result.OrderId);
        }
    }
}
