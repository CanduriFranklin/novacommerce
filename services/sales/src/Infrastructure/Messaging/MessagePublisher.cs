using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace NovaCommerce.Sales.Infrastructure.Messaging
{
    public class MessagePublisher
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public MessagePublisher(string connectionString)
        {
            var factory = new ConnectionFactory() { Uri = new System.Uri(connectionString) };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        public void Publish<T>(T message, string exchange, string routingKey)
        {
            _channel.ExchangeDeclare(exchange, ExchangeType.Topic, durable: true);

            var messageBody = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(messageBody);

            _channel.BasicPublish(exchange: exchange,
                                 routingKey: routingKey,
                                 basicProperties: null,
                                 body: body);
        }

        public void Dispose()
        {
            _channel.Close();
            _connection.Close();
        }
    }
}
