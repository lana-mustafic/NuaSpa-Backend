using System.Threading.Tasks;

namespace NuaSpa.Application.Interfaces.Messaging
{
    public interface IRabbitMQProducer
    {
        Task SendMessage<T>(T message, string queueName);
    }
}