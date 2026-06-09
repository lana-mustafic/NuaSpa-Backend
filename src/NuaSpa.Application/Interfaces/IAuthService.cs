using System.Threading;
using System.Threading.Tasks;
using NuaSpa.Application.DTOs;

namespace NuaSpa.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> LoginAsync(LoginRequest loginRequest, CancellationToken ct);
        Task<AccountProfileDto> GetMeAsync(int userId, CancellationToken ct);
        Task<AcceptInviteResponseDto> AcceptInviteAsync(AcceptTherapistInviteDto dto, CancellationToken ct);
        Task<ChangePasswordResponseDto> ChangePasswordAsync(
            int userId,
            ChangePasswordDto dto,
            string? revokeTokenJti,
            DateTime? revokeTokenExpiresUtc,
            CancellationToken ct);
        Task LogoutAsync(string jti, DateTime expiresAtUtc, CancellationToken ct);
    }
}

