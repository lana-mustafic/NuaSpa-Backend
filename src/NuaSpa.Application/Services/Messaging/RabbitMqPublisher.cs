using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NuaSpa.Application.Interfaces.Messaging;
using NuaSpa.Application.Messaging;
using RabbitMQ.Client;

namespace NuaSpa.Application.Services.Messaging;

/// <summary>
/// Long-lived RabbitMQ connection/channel (singleton). Opening a full
/// connection per publish overloads the broker and the API process.
/// </summary>
public sealed class RabbitMqPublisher : IRabbitMqPublisher, IAsyncDisposable
{
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqPublisher> _logger;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly ConnectionFactory _factory;

    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMqPublisher(IOptions<RabbitMqOptions> options, ILogger<RabbitMqPublisher> logger)
    {
        _options = options.Value;
        _logger = logger;
        _factory = RabbitMqRetry.CreateFactory(_options);
    }

    public async Task PublishAsync(string messageType, object payload, CancellationToken cancellationToken = default)
    {
        var envelope = new NuaSpaMessageEnvelope
        {
            Type = messageType,
            PayloadJson = JsonSerializer.Serialize(payload),
            CreatedAtUtc = DateTime.UtcNow,
            CorrelationId = Guid.NewGuid().ToString("N"),
        };

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(envelope));
        Exception? lastError = null;

        for (var attempt = 1; attempt <= RabbitMqRetry.MaxAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await _gate.WaitAsync(cancellationToken);
            try
            {
                await EnsureChannelAsync(cancellationToken);
                await _channel!.BasicPublishAsync(
                    exchange: string.Empty,
                    routingKey: _options.NotificationsQueue,
                    body: body,
                    cancellationToken: cancellationToken);

                _logger.LogInformation(
                    "RabbitMQ publish uspješan Type={Type} Queue={Queue} Host={Host}:{Port} CorrelationId={CorrelationId}",
                    messageType,
                    _options.NotificationsQueue,
                    _options.Host,
                    _options.Port,
                    envelope.CorrelationId);
                return;
            }
            catch (Exception ex) when (attempt < RabbitMqRetry.MaxAttempts)
            {
                lastError = ex;
                await ResetChannelAsync();
                var delay = RabbitMqRetry.DelayBeforeRetry(attempt);
                _logger.LogWarning(
                    ex,
                    "RabbitMQ publish neuspješan (pokušaj {Attempt}/{Max}). Ponovni pokušaj za {Delay}s. Type={Type} CorrelationId={CorrelationId}",
                    attempt,
                    RabbitMqRetry.MaxAttempts,
                    delay.TotalSeconds,
                    messageType,
                    envelope.CorrelationId);
            }
            catch (Exception ex)
            {
                lastError = ex;
            }
            finally
            {
                _gate.Release();
            }

            if (attempt < RabbitMqRetry.MaxAttempts)
            {
                await Task.Delay(RabbitMqRetry.DelayBeforeRetry(attempt), cancellationToken);
            }
        }

        _logger.LogError(
            lastError,
            "RabbitMQ publish NIJE USPIO Type={Type} Queue={Queue} Host={Host}:{Port} CorrelationId={CorrelationId}",
            messageType,
            _options.NotificationsQueue,
            _options.Host,
            _options.Port,
            envelope.CorrelationId);
        throw lastError ?? new InvalidOperationException("RabbitMQ publish failed.");
    }

    private async Task EnsureChannelAsync(CancellationToken cancellationToken)
    {
        if (_connection is { IsOpen: true } && _channel is { IsOpen: true })
        {
            return;
        }

        await ResetChannelAsync();

        _connection = await _factory.CreateConnectionAsync(cancellationToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await _channel.QueueDeclareAsync(
            queue: _options.NotificationsQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);
    }

    private async Task ResetChannelAsync()
    {
        if (_channel != null)
        {
            try
            {
                await _channel.DisposeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "RabbitMQ channel dispose");
            }

            _channel = null;
        }

        if (_connection != null)
        {
            try
            {
                await _connection.DisposeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "RabbitMQ connection dispose");
            }

            _connection = null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _gate.WaitAsync();
        try
        {
            await ResetChannelAsync();
        }
        finally
        {
            _gate.Release();
            _gate.Dispose();
        }
    }
}
