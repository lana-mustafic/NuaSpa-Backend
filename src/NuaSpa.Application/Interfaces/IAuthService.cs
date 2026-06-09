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
        Task<AuthResponse> RefreshAsync(RefreshTokenRequestDto dto, CancellationToken ct);
        Task LogoutAsync(
            string jti,
            DateTime expiresAtUtc,
            string? refreshToken,
            CancellationToken ct);
        Task<ForgotPasswordResponseDto> RequestPasswordResetAsync(
            ForgotPasswordRequestDto dto,
            bool includeDevResetUrl,
            CancellationToken ct);
        Task<ResetPasswordResponseDto> ResetPasswordAsync(
            ResetPasswordRequestDto dto,
            CancellationToken ct);
    }
}

