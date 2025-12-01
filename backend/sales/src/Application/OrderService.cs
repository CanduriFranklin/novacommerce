using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using NovaCommerce.Sales.Domain;
using NovaCommerce.Sales.Infrastructure.Messaging;
using NovaCommerce.Sales.Infrastructure; // Added for SalesDbContext

namespace NovaCommerce.Sales.Application
{
    public class OrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly HttpClient _httpClient;
        private readonly MessagePublisher _messagePublisher;
        private readonly IOrderCacheService _orderCacheService;
        private readonly SalesDbContext _dbContext; // Injected DbContext for tracing

        public OrderService(IOrderRepository orderRepository, HttpClient httpClient, MessagePublisher messagePublisher, IOrderCacheService orderCacheService, SalesDbContext dbContext)
        {
            _orderRepository = orderRepository;
            _httpClient = httpClient;
            _messagePublisher = messagePublisher;
            _orderCacheService = orderCacheService;
            _dbContext = dbContext;
        }

        public async Task<Order> GetOrderById(Guid id)
        {
            var order = await _orderCacheService.GetOrderAsync(id);
            if (order == null)
            {
                order = await _orderRepository.GetByIdAsync(id);
                if (order != null)
                {
                    await _orderCacheService.SetOrderAsync(order);
                }
            }
            return order;
        }

        public async Task<IEnumerable<Order>> GetAllOrders()
        {
            var orders = await _orderCacheService.GetOrdersAsync();
            if (orders == null)
            {
                orders = await _orderRepository.GetAllAsync();
                if (orders != null)
                {
                    await _orderCacheService.SetOrdersAsync(orders);
                }
            }
            return orders;
        }

        public async Task CreateOrder(Order order)
        {
            order.CreatedAt = DateTime.UtcNow;
            order.Status = OrderStatus.Pending;
            await _orderRepository.AddAsync(order);
            await _orderCacheService.SetOrderAsync(order);

            // Add trace entry
            await _dbContext.Traces.AddAsync(new Domain.Trace
            {
                AgentId = "System", // Or get from authenticated user context
                Operation = "OrderCreated",
                Entity = "Order",
                EntityId = order.OrderId,
                Timestamp = DateTime.UtcNow,
                Details = $"Order {order.OrderId} created for customer {order.CustomerId} with total {order.Total}."
            });
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateOrder(Order order)
        {
            await _orderRepository.UpdateAsync(order);
            await _orderCacheService.SetOrderAsync(order);

            // Add trace entry for order update
            await _dbContext.Traces.AddAsync(new Domain.Trace
            {
                AgentId = "System", // Or get from authenticated user context
                Operation = "OrderUpdated",
                Entity = "Order",
                EntityId = order.OrderId,
                Timestamp = DateTime.UtcNow,
                Details = $"Order {order.OrderId} updated to status {order.Status}."
            });
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> ConfirmOrder(Guid orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
            {
                return false;
            }

            foreach (var item in order.OrderItems)
            {
                var response = await _httpClient.GetAsync($"http://inventory-service/api/v1/products/{item.ProductId}");
                if (!response.IsSuccessStatusCode)
                {
                    return false;
                }

                var productJson = await response.Content.ReadAsStringAsync();
                var product = JsonSerializer.Deserialize<Product>(productJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (product.Stock < item.Quantity)
                {
                    order.Status = OrderStatus.Rejected;
                    await _orderRepository.UpdateAsync(order);
                    await _orderCacheService.SetOrderAsync(order);

                    // Add trace entry
                    await _dbContext.Traces.AddAsync(new Domain.Trace
                    {
                        AgentId = "System",
                        Operation = "OrderRejected",
                        Entity = "Order",
                        EntityId = order.OrderId,
                        Timestamp = DateTime.UtcNow,
                        Details = $"Order {order.OrderId} rejected due to insufficient stock for product {item.ProductId}."
                    });
                    await _dbContext.SaveChangesAsync();

                    return false;
                }
            }

            order.Status = OrderStatus.Confirmed;
            await _orderRepository.UpdateAsync(order);
            await _orderCacheService.SetOrderAsync(order);

            _messagePublisher.Publish(new OrderConfirmedEvent { OrderId = order.OrderId, OrderItems = order.OrderItems }, "sales", "order.confirmed");

            // Add trace entry
            await _dbContext.Traces.AddAsync(new Domain.Trace
            {
                AgentId = "System",
                Operation = "OrderConfirmed",
                Entity = "Order",
                EntityId = order.OrderId,
                Timestamp = DateTime.UtcNow,
                Details = $"Order {order.OrderId} confirmed."
            });
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> CancelOrder(Guid orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null || order.Status == OrderStatus.Cancelled || order.Status == OrderStatus.Rejected)
            {
                return false;
            }

            order.Status = OrderStatus.Cancelled;
            await _orderRepository.UpdateAsync(order);
            await _orderCacheService.SetOrderAsync(order);

            _messagePublisher.Publish(new OrderCancelledEvent { OrderId = order.OrderId, OrderItems = order.OrderItems }, "sales", "order.cancelled");

            // Add trace entry
            await _dbContext.Traces.AddAsync(new Domain.Trace
            {
                AgentId = "System",
                Operation = "OrderCancelled",
                Entity = "Order",
                EntityId = order.OrderId,
                Timestamp = DateTime.UtcNow,
                Details = $"Order {order.OrderId} cancelled."
            });
            await _dbContext.SaveChangesAsync();

            return true;
        }
    }

    // DTO for synchronous product check from Inventory service
    public class Product
    {
        public Guid ProductId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public byte[] RowVersion { get; set; }
    }

    public class OrderConfirmedEvent
    {
        public Guid OrderId { get; set; }
        public List<OrderItem> OrderItems { get; set; }
    }

    public class OrderCancelledEvent
    {
        public Guid OrderId { get; set; }
        public List<OrderItem> OrderItems { get; set; }
    }
}
