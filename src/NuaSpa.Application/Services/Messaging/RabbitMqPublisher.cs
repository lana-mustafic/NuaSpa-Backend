using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NuaSpa.Application.Interfaces.Messaging;
using NuaSpa.Application.Messaging;
using RabbitMQ.Client;

namespace NuaSpa.Application.Services.Messaging;

public sealed class RabbitMqPublisher : IRabbitMqPublisher
{
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqPublisher> _logger;

    public RabbitMqPublisher(IOptions<RabbitMqOptions> options, ILogger<RabbitMqPublisher> logger)
    {
        _options = options.Value;
        _logger = logger;
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

        var factory = new ConnectionFactory
        {
            HostName = _options.Host,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,
        };

        try
        {
            await using var connection = await factory.CreateConnectionAsync(cancellationToken);
            await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

            await channel.QueueDeclareAsync(
                queue: _options.NotificationsQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(envelope));
            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: _options.NotificationsQueue,
                body: body,
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "RabbitMQ objavljena poruka {Type} na red {Queue}",
                messageType,
                _options.NotificationsQueue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RabbitMQ publish nije uspio za tip {Type}", messageType);
            throw;
        }
    }
}
