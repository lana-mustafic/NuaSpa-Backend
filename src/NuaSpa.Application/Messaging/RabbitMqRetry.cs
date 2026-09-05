using RabbitMQ.Client;

namespace NuaSpa.Application.Messaging;

/// <summary>RS2: limited retries with exponential wait (1, 2, 4, 8 seconds).</summary>
public static class RabbitMqRetry
{
    public static readonly TimeSpan[] Backoff =
    [
        TimeSpan.FromSeconds(1),
        TimeSpan.FromSeconds(2),
        TimeSpan.FromSeconds(4),
        TimeSpan.FromSeconds(8),
    ];

    public static int MaxAttempts { get; } = Backoff.Length + 1;

    public static TimeSpan DelayBeforeRetry(int failedAttemptNumber)
    {
        var index = Math.Clamp(failedAttemptNumber - 1, 0, Backoff.Length - 1);
        return Backoff[index];
    }

    public static ConnectionFactory CreateFactory(RabbitMqOptions options) => new()
    {
        HostName = options.Host,
        Port = options.Port,
        UserName = options.UserName,
        Password = options.Password,
        AutomaticRecoveryEnabled = true,
        TopologyRecoveryEnabled = true,
        NetworkRecoveryInterval = TimeSpan.FromSeconds(8),
    };

    public static string DeadLetterQueue(string notificationsQueue) =>
        $"{notificationsQueue}.dead";
}
