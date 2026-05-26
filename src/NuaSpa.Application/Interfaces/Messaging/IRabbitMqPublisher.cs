namespace NuaSpa.Application.Interfaces.Messaging;

public interface IRabbitMqPublisher
{
    Task PublishAsync(string messageType, object payload, CancellationToken cancellationToken = default);
}
