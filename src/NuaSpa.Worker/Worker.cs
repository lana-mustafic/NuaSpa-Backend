using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace NuaSpa.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private IConnection? _connection;
    private IChannel? _channel;
    private const string QueueName = "usluge_queue";

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory { HostName = "localhost" };

        // Povezivanje na RabbitMQ (Async verzija v7+)
        _connection = await factory.CreateConnectionAsync(stoppingToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        // Deklaracija queue-a (mora biti ista kao u Produceru)
        await _channel.QueueDeclareAsync(queue: QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: stoppingToken);

        _logger.LogInformation(" [*] Worker čeka poruke na {QueueName}...", QueueName);

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            _logger.LogInformation(" [x] Primljena nova usluga: {Message}", message);

            // Ovdje bi išla tvoja logika (npr. slanje emaila ili logiranje)

            await Task.Yield();
        };

        await _channel.BasicConsumeAsync(QueueName, autoAck: true, consumer: consumer, cancellationToken: stoppingToken);

        // Drži Worker živim dok se ne ugasi aplikacija
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel != null) await _channel.CloseAsync(cancellationToken);
        if (_connection != null) await _connection.CloseAsync(cancellationToken);
        await base.StopAsync(cancellationToken);
    }
}