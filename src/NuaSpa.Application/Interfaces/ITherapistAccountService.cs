using NuaSpa.Application.DTOs;

namespace NuaSpa.Application.Interfaces;

public interface ITherapistAccountService
{
    Task<TherapistAccountStatusDto?> GetAccountStatusAsync(int zaposlenikId);
    Task<TherapistInviteResponseDto> InviteAsync(
        int zaposlenikId,
        string? emailOverride,
        int? createdByKorisnikId,
        string inviteBaseUrl);
    Task<ValidateInviteTokenDto> ValidateInviteTokenAsync(string token);
    Task<(bool Success, string Message)> AcceptInviteAsync(
        string token,
        string password);
}
