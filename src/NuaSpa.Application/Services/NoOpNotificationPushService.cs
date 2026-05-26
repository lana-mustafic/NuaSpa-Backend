using System.Threading;
using System.Threading.Tasks;
using NuaSpa.Application.Interfaces;

namespace NuaSpa.Application.Services;

/// <summary>Default no-op push (npr. Worker); API registrira SignalR implementaciju.</summary>
public sealed class NoOpNotificationPushService : INotificationPushService
{
    public Task PushUpdatedAsync(int korisnikId, CancellationToken ct = default) =>
        Task.CompletedTask;
}
