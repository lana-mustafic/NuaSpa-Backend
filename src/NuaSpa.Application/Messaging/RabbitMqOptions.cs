namespace NuaSpa.Application.Messaging;

public class RabbitMqOptions
{
    public const string SectionName = "RabbitMQ";

    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string NotificationsQueue { get; set; } = string.Empty;
}
