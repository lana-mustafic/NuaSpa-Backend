using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using NuaSpa.Application.Interfaces.Messaging;

namespace NuaSpa.Api.Services.Messaging
{
    public class RabbitMQProducer : IRabbitMQProducer
    {
        public async Task SendMessage<T>(T message, string queueName)
        {
            // Koristimo 'rabbitmq' ako API ide u Docker, ili 'localhost' ako testiraš lokalno
            var factory = new ConnectionFactory { HostName = "localhost" };

            try
            {
                using var connection = await factory.CreateConnectionAsync();
                using var channel = await connection.CreateChannelAsync();

                await channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

                var json = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(json);

                await channel.BasicPublishAsync(exchange: "", routingKey: queueName, body: body);
            }
            catch (Exception ex)
            {
                // U pravoj aplikaciji ovdje bi išao Logger
                Console.WriteLine($"RabbitMQ Error: {ex.Message}");
            }
        }
    }
}