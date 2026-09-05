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
    private const int MaxConnectRetries = 10;

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
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex) when (attempt < MaxConnectRetries)
            {
                var delay = RabbitMqRetry.DelayBeforeRetry(attempt);
                _logger.LogWarning(
                    ex,
                    "RabbitMQ nije dostupan (pokušaj {Attempt}/{Max}). Ponovni pokušaj za {Delay}s…",
                    attempt,
                    MaxConnectRetries,
                    delay.TotalSeconds);
                await Task.Delay(delay, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "RabbitMQ konekcija nije uspjela nakon {Max} pokušaja.",
                    MaxConnectRetries);
            }
        }
    }

    private async Task ConnectAndConsumeAsync(CancellationToken stoppingToken)
    {
        var factory = RabbitMqRetry.CreateFactory(_options);

        _connection = await factory.CreateConnectionAsync(stoppingToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        var deadQueue = RabbitMqRetry.DeadLetterQueue(_options.NotificationsQueue);
        await _channel.QueueDeclareAsync(
            queue: deadQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken);

        await _channel.QueueDeclareAsync(
            queue: _options.NotificationsQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken);

        await _channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: 1,
            global: false,
            cancellationToken: stoppingToken);

        _logger.LogInformation(
            "Worker povezan na RabbitMQ. Slušam red {Queue} ({Host}:{Port})",
            _options.NotificationsQueue,
            _options.Host,
            _options.Port);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            await HandleDeliveryAsync(ea, stoppingToken);
        };

        await _channel.BasicConsumeAsync(
            queue: _options.NotificationsQueue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task HandleDeliveryAsync(
        BasicDeliverEventArgs ea,
        CancellationToken stoppingToken)
    {
        var deliveryTag = ea.DeliveryTag;
        var channel = _channel;
        if (channel == null)
        {
            return;
        }

        try
        {
            var json = Encoding.UTF8.GetString(ea.Body.ToArray());
            var envelope = JsonSerializer.Deserialize<NuaSpaMessageEnvelope>(json);
            if (envelope == null || string.IsNullOrWhiteSpace(envelope.Type))
            {
                _logger.LogWarning(
                    "Neispravna poruka (DeliveryTag={DeliveryTag}, prazan envelope) — dead-letter",
                    deliveryTag);
                await MoveToDeadLetterAsync(channel, ea.Body, stoppingToken);
                await channel.BasicAckAsync(deliveryTag, multiple: false, cancellationToken: stoppingToken);
                return;
            }

            _logger.LogInformation(
                "Primljena poruka Type={Type} CorrelationId={CorrelationId} DeliveryTag={DeliveryTag}",
                envelope.Type,
                envelope.CorrelationId,
                deliveryTag);

            await DispatchWithRetryAsync(envelope, stoppingToken);
            await channel.BasicAckAsync(deliveryTag, multiple: false, cancellationToken: stoppingToken);

            _logger.LogInformation(
                "Poruka obrađena Type={Type} CorrelationId={CorrelationId}",
                envelope.Type,
                envelope.CorrelationId);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            try
            {
                await channel.BasicNackAsync(
                    deliveryTag,
                    multiple: false,
                    requeue: true,
                    cancellationToken: CancellationToken.None);
            }
            catch (Exception nackEx)
            {
                _logger.LogWarning(nackEx, "NACK requeue nije uspio pri zaustavljanju Worker-a");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Poruka trajno neuspješna nakon {Attempts} pokušaja DeliveryTag={DeliveryTag} — dead-letter {DeadQueue}",
                RabbitMqRetry.MaxAttempts,
                deliveryTag,
                RabbitMqRetry.DeadLetterQueue(_options.NotificationsQueue));
            try
            {
                await MoveToDeadLetterAsync(channel, ea.Body, CancellationToken.None);
                await channel.BasicAckAsync(deliveryTag, multiple: false, cancellationToken: CancellationToken.None);
            }
            catch (Exception moveEx)
            {
                _logger.LogError(moveEx, "Dead-letter nije uspio — NACK bez requeue");
                await channel.BasicNackAsync(
                    deliveryTag,
                    multiple: false,
                    requeue: false,
                    cancellationToken: CancellationToken.None);
            }
        }
    }

    private async Task DispatchWithRetryAsync(
        NuaSpaMessageEnvelope envelope,
        CancellationToken stoppingToken)
    {
        Exception? lastError = null;
        for (var attempt = 1; attempt <= RabbitMqRetry.MaxAttempts; attempt++)
        {
            stoppingToken.ThrowIfCancellationRequested();
            try
            {
                await _dispatcher.DispatchAsync(envelope, stoppingToken);
                return;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex) when (attempt < RabbitMqRetry.MaxAttempts)
            {
                lastError = ex;
                var delay = RabbitMqRetry.DelayBeforeRetry(attempt);
                _logger.LogWarning(
                    ex,
                    "Transient greška obrade Type={Type} CorrelationId={CorrelationId} (pokušaj {Attempt}/{Max}). Ponovni pokušaj za {Delay}s",
                    envelope.Type,
                    envelope.CorrelationId,
                    attempt,
                    RabbitMqRetry.MaxAttempts,
                    delay.TotalSeconds);
                await Task.Delay(delay, stoppingToken);
            }
        }

        throw lastError ?? new InvalidOperationException("Message dispatch failed.");
    }

    private async Task MoveToDeadLetterAsync(
        IChannel channel,
        ReadOnlyMemory<byte> body,
        CancellationToken cancellationToken)
    {
        var deadQueue = RabbitMqRetry.DeadLetterQueue(_options.NotificationsQueue);
        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: deadQueue,
            body: body,
            cancellationToken: cancellationToken);
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
