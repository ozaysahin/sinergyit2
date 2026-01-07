using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Nest;

Console.WriteLine("Log Consumer başlatılıyor...");

// elastic
var elasticClient = new ElasticClient(new ConnectionSettings(new Uri("http://localhost:19200")));

// rabbitmq
var factory = new ConnectionFactory
{
    HostName = "localhost",
    UserName = "guest",
    Password = "guest",
    Port = 5672
};

var connection = await factory.CreateConnectionAsync();
var channel = await connection.CreateChannelAsync();

await channel.ExchangeDeclareAsync(
    exchange: "logs-exchange",
    type: "topic",
    durable: true,
    autoDelete: false
);

var queueA = "servicea-logs-queue";
await channel.QueueDeclareAsync(queue: queueA, durable: true, exclusive: false, autoDelete: false);
await channel.QueueBindAsync(queue: queueA, exchange: "logs-exchange", routingKey: "servicea");

var consumerA = new AsyncEventingBasicConsumer(channel);
consumerA.ReceivedAsync += async (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    Console.WriteLine($"[ServiceA] Mesaj alındı: {message}");

    try
    {
        var logEntry = JsonSerializer.Deserialize<LogEntry>(message);

        // servicea yaz
        await elasticClient.IndexAsync(logEntry, idx => idx.Index("project-servicea-logs"));

        // ortak indexe yaz
        await elasticClient.IndexAsync(logEntry, idx => idx.Index("project-microservices-logs"));

        Console.WriteLine($"[ServiceA] Elasticsearch'e yazıldı (2 index)");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Hata: {ex.Message}");
    }
};

await channel.BasicConsumeAsync(queue: queueA, autoAck: true, consumer: consumerA);
Console.WriteLine($"✓ ServiceA kuyrugu dinleniyor: {queueA}");

var queueB = "serviceb-logs-queue";
await channel.QueueDeclareAsync(queue: queueB, durable: true, exclusive: false, autoDelete: false);
await channel.QueueBindAsync(queue: queueB, exchange: "logs-exchange", routingKey: "serviceb");

var consumerB = new AsyncEventingBasicConsumer(channel);
consumerB.ReceivedAsync += async (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    Console.WriteLine($"[ServiceB] Mesaj alındı: {message}");

    try
    {
        var logEntry = JsonSerializer.Deserialize<LogEntry>(message);

        // serviceb indexine yaz
        await elasticClient.IndexAsync(logEntry, idx => idx.Index("project-serviceb-logs"));

        // ortak indexe yazdırma
        await elasticClient.IndexAsync(logEntry, idx => idx.Index("project-microservices-logs"));

        Console.WriteLine($"[ServiceB] Elasticsearch'e yazıldı (2 index)");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Hata: {ex.Message}");
    }
};

await channel.BasicConsumeAsync(queue: queueB, autoAck: true, consumer: consumerB);
Console.WriteLine($"✓ ServiceB kuyrugu dinleniyor: {queueB}");

Console.WriteLine("\n=== Consumer çalışıyor ===");
Console.WriteLine("- project-servicea-logs");
Console.WriteLine("- project-serviceb-logs");
Console.WriteLine("- project-microservices-logs (ortak)");
Console.WriteLine("\nDurdurmak için bir tuşa basın...\n");
Console.ReadLine();

public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public string Level { get; set; }
    public string Message { get; set; }
    public string Service { get; set; }
}