using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using NuaSpa.Api.Extensions;

namespace NuaSpa.Api.Hubs;

[Authorize]
public class NotificationsHub : Hub
{
    public static string UserGroup(int korisnikId) => $"user:{korisnikId}";

    public override async Task OnConnectedAsync()
    {
        var userId = 0;
        if (Context.User != null && Context.User.TryGetNuaSpaUserId(out var uid))
        {
            userId = uid;
        }

        if (userId > 0)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, UserGroup(userId));
        }

        await base.OnConnectedAsync();
    }
}
