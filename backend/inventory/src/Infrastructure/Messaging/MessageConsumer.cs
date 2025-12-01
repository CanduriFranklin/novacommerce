using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NovaCommerce.Inventory.Application;
using System.Text.Json;
using System.Collections.Generic;
using NovaCommerce.Inventory.Domain;
using NovaCommerce.Inventory.Application.Dtos; // Using the new DTOs

namespace NovaCommerce.Inventory.Infrastructure.Messaging
{
    public class MessageConsumer : BackgroundService
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly IServiceProvider _serviceProvider;

        public MessageConsumer(IServiceProvider serviceProvider, string connectionString)
        {
            _serviceProvider = serviceProvider;
            var factory = new ConnectionFactory() { Uri = new Uri(connectionString) };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _channel.ExchangeDeclare("sales", ExchangeType.Topic, durable: true);
            var queueName = _channel.QueueDeclare().QueueName;

            // Bind for OrderConfirmed events
            _channel.QueueBind(queue: queueName,
                              exchange: "sales",
                              routingKey: "order.confirmed");

            // Bind for OrderCancelled events
            _channel.QueueBind(queue: queueName,
                              exchange: "sales",
                              routingKey: "order.cancelled");

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var routingKey = ea.RoutingKey;
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
                    var productService = scope.ServiceProvider.GetRequiredService<ProductService>();

                    if (routingKey == "order.confirmed")
                    {
                        var orderConfirmedEvent = JsonSerializer.Deserialize<OrderConfirmedEvent>(message, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (await dbContext.ProcessedEvents.FindAsync(orderConfirmedEvent.OrderId, "OrderConfirmed") != null)
                        {
                            _channel.BasicAck(ea.DeliveryTag, false);
                            return;
                        }

                        foreach (var item in orderConfirmedEvent.OrderItems)
                        {
                            var product = await productService.GetProductById(item.ProductId);
                            if (product != null)
                            {
                                product.Stock -= item.Quantity;
                                await productService.UpdateProduct(product, null); // Pass null for image as it's not relevant here
                            }
                        }

                        await dbContext.ProcessedEvents.AddAsync(new ProcessedEvent { OrderId = orderConfirmedEvent.OrderId, EventType = "OrderConfirmed", ProcessedAt = DateTime.UtcNow });
                        await dbContext.SaveChangesAsync();
                    }
                    else if (routingKey == "order.cancelled")
                    {
                        var orderCancelledEvent = JsonSerializer.Deserialize<OrderCancelledEvent>(message, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (await dbContext.ProcessedEvents.FindAsync(orderCancelledEvent.OrderId, "OrderCancelled") != null)
                        {
                            _channel.BasicAck(ea.DeliveryTag, false);
                            return;
                        }

                        foreach (var item in orderCancelledEvent.OrderItems)
                        {
                            var product = await productService.GetProductById(item.ProductId);
                            if (product != null)
                            {
                                product.Stock += item.Quantity; // Increase stock for cancelled order
                                await productService.UpdateProduct(product, null); // Pass null for image as it's not relevant here
                            }
                        }

                        await dbContext.ProcessedEvents.AddAsync(new ProcessedEvent { OrderId = orderCancelledEvent.OrderId, EventType = "OrderCancelled", ProcessedAt = DateTime.UtcNow });
                        await dbContext.SaveChangesAsync();
                    }
                }
                _channel.BasicAck(ea.DeliveryTag, false);
            };
            _channel.BasicConsume(queue: queueName,
                                 autoAck: false,
                                 consumer: consumer);

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }
}
