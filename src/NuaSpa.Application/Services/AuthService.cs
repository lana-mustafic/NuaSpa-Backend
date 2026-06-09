using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NuaSpa.Application.Common;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.Exceptions;
using NuaSpa.Application.Interfaces;
using NuaSpa.Domain;
using NuaSpa.Domain.Entities;
using NuaSpa.Domain.Enums;

namespace NuaSpa.Application.Services;

public class AuthService : IAuthService
{
    private const string InvalidCredentialsMessage = "Invalid username or password.";

    private readonly UserManager<Korisnik> _userManager;
    private readonly ITokenService _tokenService;
    private readonly ITokenRevocationService _tokenRevocationService;
    private readonly ITherapistAccountService _therapistAccountService;
    private readonly NuaSpaContext _context;

    public AuthService(
        UserManager<Korisnik> userManager,
        ITokenService tokenService,
        ITokenRevocationService tokenRevocationService,
        ITherapistAccountService therapistAccountService,
        NuaSpaContext context)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _tokenRevocationService = tokenRevocationService;
        _therapistAccountService = therapistAccountService;
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
        var issued = _tokenService.CreateToken(user, roles);

        return new AuthResponse
        {
            Token = issued.Token,
            Username = user.UserName!,
            Expiration = issued.ExpiresAtUtc.ToLocalTime(),
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

    public async Task LogoutAsync(string jti, DateTime expiresAtUtc, CancellationToken ct)
    {
        await _tokenRevocationService.RevokeAsync(jti, expiresAtUtc, ct);
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
        var issued = _tokenService.CreateToken(user, roles);

        return new AcceptInviteResponseDto
        {
            Message = message,
            Token = issued.Token,
            Username = user.UserName ?? string.Empty,
            Expiration = issued.ExpiresAtUtc.ToLocalTime(),
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

        var roles = await _userManager.GetRolesAsync(user);
        var issued = _tokenService.CreateToken(user, roles);

        return new ChangePasswordResponseDto
        {
            Message = "Password changed successfully.",
            Token = issued.Token,
            Expiration = issued.ExpiresAtUtc.ToLocalTime(),
        };
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

