using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using NuaSpa.Application.Messaging;
using NuaSpa.Worker.Messaging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NuaSpa.Worker;

/// <summary>
/// Pomoćni mikroservis: prima poruke iz RabbitMQ i izvršava asinhrone zadatke (e-mail, notifikacije).
/// </summary>
public sealed class RabbitMqNotificationConsumer : BackgroundService
{
    private readonly RabbitMqOptions _options;
    private readonly NotificationMessageDispatcher _dispatcher;
    private readonly ILogger<RabbitMqNotificationConsumer> _logger;
    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMqNotificationConsumer(
        IOptions<RabbitMqOptions> options,
        NotificationMessageDispatcher dispatcher,
        ILogger<RabbitMqNotificationConsumer> logger)
    {
        _options = options.Value;
        _dispatcher = dispatcher;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.Host,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,
        };

        _connection = await factory.CreateConnectionAsync(stoppingToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await _channel.QueueDeclareAsync(
            queue: _options.NotificationsQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken);

        _logger.LogInformation(
            "NuaSpa Worker sluša red {Queue} na {Host}:{Port}",
            _options.NotificationsQueue,
            _options.Host,
            _options.Port);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var envelope = JsonSerializer.Deserialize<NuaSpaMessageEnvelope>(json);
                if (envelope == null || string.IsNullOrWhiteSpace(envelope.Type))
                {
                    _logger.LogWarning("Primljena neispravna poruka (prazan envelope).");
                    return;
                }

                await _dispatcher.DispatchAsync(envelope, stoppingToken);
                await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri obradi poruke — NACK bez requeue.");
                await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false, cancellationToken: stoppingToken);
            }
        };

        await _channel.BasicConsumeAsync(
            queue: _options.NotificationsQueue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel != null)
        {
            await _channel.CloseAsync(cancellationToken);
        }

        if (_connection != null)
        {
            await _connection.CloseAsync(cancellationToken);
        }

        await base.StopAsync(cancellationToken);
    }
}
