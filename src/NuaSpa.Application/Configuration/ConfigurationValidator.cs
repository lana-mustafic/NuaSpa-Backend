using Microsoft.Extensions.Configuration;
using NuaSpa.Application.Messaging;

namespace NuaSpa.Application.Configuration;

public static class ConfigurationValidator
{
    public static RabbitMqOptions RequireRabbitMq(IConfiguration configuration)
    {
        var options = new RabbitMqOptions();
        configuration.GetSection(RabbitMqOptions.SectionName).Bind(options);

        if (string.IsNullOrWhiteSpace(options.Host))
        {
            throw new InvalidOperationException(
                "RabbitMQ__Host nije postavljen. Postavite RabbitMQ vrijednosti u .env (vidi .env.example).");
        }

        if (options.Port <= 0)
        {
            throw new InvalidOperationException("RabbitMQ__Port mora biti postavljen u .env.");
        }

        if (string.IsNullOrWhiteSpace(options.UserName) || string.IsNullOrWhiteSpace(options.Password))
        {
            throw new InvalidOperationException(
                "RabbitMQ__UserName i RabbitMQ__Password moraju biti postavljeni u .env.");
        }

        if (string.IsNullOrWhiteSpace(options.NotificationsQueue))
        {
            throw new InvalidOperationException(
                "RabbitMQ__NotificationsQueue mora biti postavljen u .env.");
        }

        return options;
    }
}
