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
/// Odvojen od NuaSpa.Api procesa — zadovoljava zahtjev zasebnog Worker kontejnera.
/// </summary>
public sealed class RabbitMqNotificationConsumer : BackgroundService
{
    private const int MaxConnectRetries = 36;

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
        _logger.LogInformation(
            "NuaSpa Worker pokrenut. Cilj: red={Queue}, broker={Host}:{Port}",
            _options.NotificationsQueue,
            _options.Host,
            _options.Port);

        for (var attempt = 1; attempt <= MaxConnectRetries && !stoppingToken.IsCancellationRequested; attempt++)
        {
            try
            {
                await ConnectAndConsumeAsync(stoppingToken);
                return;
            }
            catch (Exception ex) when (attempt < MaxConnectRetries)
            {
                _logger.LogWarning(
                    ex,
                    "RabbitMQ nije dostupan (pokušaj {Attempt}/{Max}). Ponovni pokušaj za 5s…",
                    attempt,
                    MaxConnectRetries);
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task ConnectAndConsumeAsync(CancellationToken stoppingToken)
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
            "Worker povezan na RabbitMQ. Slušam red {Queue} ({Host}:{Port})",
            _options.NotificationsQueue,
            _options.Host,
            _options.Port);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            var deliveryTag = ea.DeliveryTag;
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var envelope = JsonSerializer.Deserialize<NuaSpaMessageEnvelope>(json);
                if (envelope == null || string.IsNullOrWhiteSpace(envelope.Type))
                {
                    _logger.LogWarning(
                        "Neispravna poruka (DeliveryTag={DeliveryTag}, prazan envelope)",
                        deliveryTag);
                    await _channel.BasicAckAsync(deliveryTag, multiple: false, cancellationToken: stoppingToken);
                    return;
                }

                _logger.LogInformation(
                    "Primljena poruka Type={Type} CorrelationId={CorrelationId} DeliveryTag={DeliveryTag}",
                    envelope.Type,
                    envelope.CorrelationId,
                    deliveryTag);

                await _dispatcher.DispatchAsync(envelope, stoppingToken);
                await _channel.BasicAckAsync(deliveryTag, multiple: false, cancellationToken: stoppingToken);

                _logger.LogInformation(
                    "Poruka obrađena Type={Type} CorrelationId={CorrelationId}",
                    envelope.Type,
                    envelope.CorrelationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Greška pri obradi poruke DeliveryTag={DeliveryTag} — NACK bez requeue",
                    deliveryTag);
                await _channel.BasicNackAsync(
                    deliveryTag,
                    multiple: false,
                    requeue: false,
                    cancellationToken: stoppingToken);
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
        _logger.LogInformation("NuaSpa Worker zaustavljanje…");
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
