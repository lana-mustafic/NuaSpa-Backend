using System.Threading;
using System.Threading.Tasks;
using NuaSpa.Application.DTOs;

namespace NuaSpa.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> LoginAsync(LoginRequest loginRequest, CancellationToken ct);
        Task<string> AcceptInviteAsync(AcceptTherapistInviteDto dto, CancellationToken ct);
    }
}

