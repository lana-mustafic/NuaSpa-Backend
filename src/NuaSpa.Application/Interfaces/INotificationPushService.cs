using System.Threading;
using System.Threading.Tasks;

namespace NuaSpa.Application.Interfaces;

public interface INotificationPushService
{
    Task PushUpdatedAsync(int korisnikId, CancellationToken ct = default);
}
