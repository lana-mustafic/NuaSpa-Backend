using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NuaSpa.Application.Common;
using NuaSpa.Application.Configuration;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.Exceptions;
using NuaSpa.Application.Interfaces;
using NuaSpa.Application.Interfaces.Messaging;
using NuaSpa.Application.Messaging.Messages;
using NuaSpa.Domain;
using NuaSpa.Domain.Entities;
using NuaSpa.Domain.Enums;

namespace NuaSpa.Application.Services;

public class AuthService : IAuthService
{
    private const string InvalidCredentialsMessage = "Invalid username or password.";
    private const string PasswordResetAcknowledgement =
        "If an account exists with that email, you will receive password reset instructions shortly.";

    private readonly UserManager<Korisnik> _userManager;
    private readonly ITokenService _tokenService;
    private readonly ITokenRevocationService _tokenRevocationService;
    private readonly ITherapistAccountService _therapistAccountService;
    private readonly INotificationPublisher _notificationPublisher;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly PasswordResetOptions _passwordResetOptions;
    private readonly NuaSpaContext _context;

    public AuthService(
        UserManager<Korisnik> userManager,
        ITokenService tokenService,
        ITokenRevocationService tokenRevocationService,
        ITherapistAccountService therapistAccountService,
        INotificationPublisher notificationPublisher,
        IRefreshTokenService refreshTokenService,
        IOptions<PasswordResetOptions> passwordResetOptions,
        NuaSpaContext context)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _tokenRevocationService = tokenRevocationService;
        _therapistAccountService = therapistAccountService;
        _notificationPublisher = notificationPublisher;
        _refreshTokenService = refreshTokenService;
        _passwordResetOptions = passwordResetOptions.Value;
        _context = context;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest loginRequest, CancellationToken ct)
    {
        var user = await _userManager.FindByNameAsync(loginRequest.Username)
            ?? await _userManager.FindByEmailAsync(loginRequest.Username);

        if (user == null)
        {
            throw new UnauthorizedException(InvalidCredentialsMessage);
        }

        await EnsureLockoutEnabledAsync(user);

        if (await _userManager.IsLockedOutAsync(user))
        {
            throw new UnauthorizedException(
                "Account is temporarily locked due to too many failed sign-in attempts. Try again later or contact your administrator.");
        }

        if (!user.Status)
        {
            throw new UnauthorizedException("Account is deactivated. Contact your spa administrator.");
        }

        if (!await _userManager.HasPasswordAsync(user))
        {
            throw new UnauthorizedException(
                "Portal access is not activated yet. Open your invitation link to set a password.");
        }

        var result = await _userManager.CheckPasswordAsync(user, loginRequest.Password);
        if (!result)
        {
            await _userManager.AccessFailedAsync(user);
            throw new UnauthorizedException(InvalidCredentialsMessage);
        }

        await _userManager.ResetAccessFailedCountAsync(user);

        // Ako je user terapeut, dodatna provjera statusa terapeuta.
        if (await _userManager.IsInRoleAsync(user, RoleConstants.Zaposlenik) && user.ZaposlenikId is > 0)
        {
            var zStatus = await _context.Zaposlenici.AsNoTracking()
                .Where(z => z.Id == user.ZaposlenikId)
                .Select(z => z.Status)
                .FirstOrDefaultAsync(ct);

            if (zStatus != ZaposlenikStatus.Active)
            {
                throw new UnauthorizedException(
                    "Your therapist profile is not active. Contact your spa administrator.");
            }
        }

        var roles = await _userManager.GetRolesAsync(user);
        return await CreateSessionAsync(user, roles, ct);
    }

    public async Task<AuthResponse> RefreshAsync(RefreshTokenRequestDto dto, CancellationToken ct)
    {
        var rotation = await _refreshTokenService.RotateAsync(dto.RefreshToken, ct);
        if (rotation.ReuseDetected)
        {
            throw new UnauthorizedException("Session reuse detected. Please sign in again.");
        }

        if (!rotation.Success || rotation.UserId is not int userId || rotation.RefreshToken is null)
        {
            throw new UnauthorizedException("Session expired. Please sign in again.");
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null || !user.Status)
        {
            throw new UnauthorizedException("Session expired. Please sign in again.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var issued = _tokenService.CreateToken(user, roles);

        return new AuthResponse
        {
            Token = issued.Token,
            RefreshToken = rotation.RefreshToken.Token,
            Username = user.UserName ?? string.Empty,
            Expiration = issued.ExpiresAtUtc.ToLocalTime(),
            RefreshExpiration = rotation.RefreshToken.ExpiresAtUtc.ToLocalTime(),
        };
    }

    public async Task<AccountProfileDto> GetMeAsync(int userId, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new NotFoundException("Korisnik nije pronađen.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var hasPassword = await _userManager.HasPasswordAsync(user);

        return new AccountProfileDto
        {
            UserId = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email,
            FirstName = user.Ime,
            LastName = user.Prezime,
            Roles = roles.ToList(),
            IsActive = user.Status,
            HasPassword = hasPassword,
            ZaposlenikId = user.ZaposlenikId,
        };
    }

    public async Task LogoutAsync(
        string jti,
        DateTime expiresAtUtc,
        string? refreshToken,
        CancellationToken ct)
    {
        await _tokenRevocationService.RevokeAsync(jti, expiresAtUtc, ct);
        await _refreshTokenService.RevokeByPlainTokenAsync(refreshToken, ct);
    }

    public async Task<AcceptInviteResponseDto> AcceptInviteAsync(
        AcceptTherapistInviteDto dto,
        CancellationToken ct)
    {
        if (dto.Password != dto.ConfirmPassword)
        {
            throw new BusinessRuleException("Passwords do not match.");
        }

        var (success, message, userId) = await _therapistAccountService.AcceptInviteAsync(
            dto.Token,
            dto.Password);
        if (!success || userId is not int activatedUserId)
        {
            throw new BusinessRuleException(message);
        }

        var user = await _userManager.FindByIdAsync(activatedUserId.ToString());
        if (user == null)
        {
            throw new NotFoundException("User account not found after activation.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var session = await CreateSessionAsync(user, roles, ct);

        return new AcceptInviteResponseDto
        {
            Message = message,
            Token = session.Token,
            RefreshToken = session.RefreshToken,
            Username = session.Username,
            Expiration = session.Expiration,
            RefreshExpiration = session.RefreshExpiration,
        };
    }

    public async Task<ChangePasswordResponseDto> ChangePasswordAsync(
        int userId,
        ChangePasswordDto dto,
        string? revokeTokenJti,
        DateTime? revokeTokenExpiresUtc,
        CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new NotFoundException("Korisnik nije pronađen.");
        }

        if (!await _userManager.HasPasswordAsync(user))
        {
            throw new BusinessRuleException(
                "Lozinka još nije postavljena. Koristite link za pozivnicu.");
        }

        var currentOk = await _userManager.CheckPasswordAsync(user, dto.StaraLozinka);
        if (!currentOk)
        {
            throw new BusinessRuleException("Trenutna lozinka nije ispravna.");
        }

        var result = await _userManager.ChangePasswordAsync(
            user, dto.StaraLozinka, dto.NovaLozinka);
        if (!result.Succeeded)
        {
            throw new BusinessRuleException(
                string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        if (!string.IsNullOrWhiteSpace(revokeTokenJti) && revokeTokenExpiresUtc.HasValue)
        {
            await _tokenRevocationService.RevokeAsync(
                revokeTokenJti,
                revokeTokenExpiresUtc.Value,
                ct);
        }

        await _refreshTokenService.RevokeAllForUserAsync(user.Id, ct);

        var roles = await _userManager.GetRolesAsync(user);
        var session = await CreateSessionAsync(user, roles, ct);

        return new ChangePasswordResponseDto
        {
            Message = "Password changed successfully.",
            Token = session.Token,
            RefreshToken = session.RefreshToken,
            Expiration = session.Expiration,
            RefreshExpiration = session.RefreshExpiration,
        };
    }

    public async Task<ForgotPasswordResponseDto> RequestPasswordResetAsync(
        ForgotPasswordRequestDto dto,
        bool includeDevResetUrl,
        CancellationToken ct)
    {
        var response = new ForgotPasswordResponseDto
        {
            Message = PasswordResetAcknowledgement,
        };

        var email = dto.Email.Trim();
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null || !user.Status || !await _userManager.HasPasswordAsync(user))
        {
            return response;
        }

        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var expiresAtUtc = DateTime.UtcNow.AddHours(_passwordResetOptions.TokenLifespanHours);
        var resetUrl = BuildPasswordResetUrl(email, resetToken);
        var displayName = $"{user.Ime} {user.Prezime}".Trim();
        if (string.IsNullOrWhiteSpace(displayName))
        {
            displayName = user.UserName ?? email;
        }

        try
        {
            await _notificationPublisher.PublishPasswordResetAsync(
                new PasswordResetEmailMessage
                {
                    ToEmail = email,
                    DisplayName = displayName,
                    ResetUrl = resetUrl,
                    ExpiresAtUtc = expiresAtUtc,
                },
                ct);
        }
        catch
        {
            // Do not reveal whether the account exists.
            return response;
        }

        if (includeDevResetUrl)
        {
            response.DevResetUrl = resetUrl;
        }

        return response;
    }

    public async Task<ResetPasswordResponseDto> ResetPasswordAsync(
        ResetPasswordRequestDto dto,
        CancellationToken ct)
    {
        if (dto.Password != dto.ConfirmPassword)
        {
            throw new BusinessRuleException("Passwords do not match.");
        }

        var email = dto.Email.Trim();
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            throw new BusinessRuleException("Invalid or expired reset link. Request a new password reset.");
        }

        if (!user.Status)
        {
            throw new BusinessRuleException("Account is deactivated. Contact your spa administrator.");
        }

        var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.Password);
        if (!result.Succeeded)
        {
            throw new BusinessRuleException(
                "Invalid or expired reset link. Request a new password reset.");
        }

        await _userManager.ResetAccessFailedCountAsync(user);
        user.LockoutEnabled = true;
        await _userManager.UpdateAsync(user);

        await _refreshTokenService.RevokeAllForUserAsync(user.Id, ct);

        var roles = await _userManager.GetRolesAsync(user);
        var session = await CreateSessionAsync(user, roles, ct);

        return new ResetPasswordResponseDto
        {
            Message = "Password reset successfully. Signing you in…",
            Token = session.Token,
            RefreshToken = session.RefreshToken,
            Username = session.Username,
            Expiration = session.Expiration,
            RefreshExpiration = session.RefreshExpiration,
        };
    }

    private async Task<AuthResponse> CreateSessionAsync(
        Korisnik user,
        IList<string> roles,
        CancellationToken ct)
    {
        var issued = _tokenService.CreateToken(user, roles);
        var refresh = await _refreshTokenService.IssueAsync(user.Id, familyId: null, ct);

        return new AuthResponse
        {
            Token = issued.Token,
            RefreshToken = refresh.Token,
            Username = user.UserName ?? string.Empty,
            Expiration = issued.ExpiresAtUtc.ToLocalTime(),
            RefreshExpiration = refresh.ExpiresAtUtc.ToLocalTime(),
        };
    }

    private string BuildPasswordResetUrl(string email, string token)
    {
        var baseUrl = _passwordResetOptions.BaseUrl.Trim();
        var separator = baseUrl.Contains('?', StringComparison.Ordinal) ? '&' : '?';
        return $"{baseUrl}{separator}email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";
    }

    private async Task EnsureLockoutEnabledAsync(Korisnik user)
    {
        if (user.LockoutEnabled)
        {
            return;
        }

        user.LockoutEnabled = true;
        await _userManager.UpdateAsync(user);
    }
}

