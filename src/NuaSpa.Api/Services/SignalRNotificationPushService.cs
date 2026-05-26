using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using NuaSpa.Api.Hubs;
using NuaSpa.Application.Interfaces;

namespace NuaSpa.Api.Services;

public sealed class SignalRNotificationPushService : INotificationPushService
{
    private readonly IHubContext<NotificationsHub> _hub;

    public SignalRNotificationPushService(IHubContext<NotificationsHub> hub)
    {
        _hub = hub;
    }

    public Task PushUpdatedAsync(int korisnikId, CancellationToken ct = default) =>
        _hub.Clients
            .Group(NotificationsHub.UserGroup(korisnikId))
            .SendAsync("notificationsUpdated", cancellationToken: ct);
}
