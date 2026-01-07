using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace sinergyit.ServiceA.API.ApiHelper
{
    public class RabbitMQLogger
    {
        private readonly IConnection _connection;
        private readonly IChannel _channel;
        private readonly string _routingKey;

        public RabbitMQLogger(string routingKey)
        {
            _routingKey = routingKey;

            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest",
                Port = 5672
            };

            _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
            _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();

            _channel.ExchangeDeclareAsync(
                exchange: "logs-exchange",
                type: "topic",
                durable: true,
                autoDelete: false
            ).GetAwaiter().GetResult();
        }

        public void LogInformation(string message)
        {
            SendLog("Information", message);
        }

        public void LogWarning(string message)
        {
            SendLog("Warning", message);
        }

        public void LogError(string message)
        {
            SendLog("Error", message);
        }

        private void SendLog(string level, string message)
        {
            var logData = new
            {
                Timestamp = DateTime.Now,
                Level = level,
                Message = message,
                Service = _routingKey
            };

            var jsonMessage = JsonSerializer.Serialize(logData);
            var body = Encoding.UTF8.GetBytes(jsonMessage);

            _channel.BasicPublishAsync(
                exchange: "logs-exchange",
                routingKey: _routingKey,
                body: body
            ).GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            _channel?.CloseAsync().GetAwaiter().GetResult();
            _connection?.CloseAsync().GetAwaiter().GetResult();
        }
    }
}