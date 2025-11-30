using RabbitMQ.Client;
using Serilog;
using System.Text;
using System.Threading.Tasks;

namespace OutboxWorker.Infrastructure
{
    public class RabbitMqPublisher : IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _connectionString;

        public RabbitMqPublisher(string connectionString)
        {
            _connectionString = connectionString;
            var factory = new ConnectionFactory { Uri = new Uri(connectionString) };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ConfirmSelect(); // Enable publisher confirms
            _channel.BasicAcks += (sender, ea) =>
            {
                Log.Information("Message with tag {DeliveryTag} acknowledged by RabbitMQ.", ea.DeliveryTag);
            };
            _channel.BasicNacks += (sender, ea) =>
            {
                Log.Warning("Message with tag {DeliveryTag} negatively acknowledged by RabbitMQ. Requeue: {Requeue}", ea.DeliveryTag, ea.Multiple);
            };
            _channel.BasicReturn += (sender, ea) =>
            {
                var body = Encoding.UTF8.GetString(ea.Body.ToArray());
                Log.Warning("Message returned from RabbitMQ. ReplyText: {ReplyText}, Exchange: {Exchange}, RoutingKey: {RoutingKey}, Body: {Body}",
                    ea.ReplyText, ea.Exchange, ea.RoutingKey, body);
            };
        }

        public async Task PublishMessage(string exchange, string routingKey, string message)
        {
            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(exchange: exchange,
                                 routingKey: routingKey,
                                 basicProperties: null,
                                 body: body);
            await _channel.WaitForConfirmsAsync(); // Wait for publisher confirms
            Log.Information("Message published to exchange '{Exchange}' with routing key '{RoutingKey}'.", exchange, routingKey);
        }

        public void Dispose()
        {
            _channel.Dispose();
            _connection.Dispose();
        }
    }
}
