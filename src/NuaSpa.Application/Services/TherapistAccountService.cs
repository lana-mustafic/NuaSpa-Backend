using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.Interfaces;
using NuaSpa.Domain;
using NuaSpa.Domain.Entities;

namespace NuaSpa.Application.Services;

public class TherapistAccountService : ITherapistAccountService
{
    private const int InviteValidHours = 72;
    private const int MinPasswordLength = 8;

    private readonly NuaSpaContext _context;
    private readonly UserManager<Korisnik> _userManager;

    public TherapistAccountService(NuaSpaContext context, UserManager<Korisnik> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<TherapistAccountStatusDto?> GetAccountStatusAsync(int zaposlenikId)
    {
        var z = await _context.Zaposlenici.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == zaposlenikId);
        if (z == null) return null;

        var user = await _context.Users.AsNoTracking()
            .FirstOrDefaultAsync(k => k.ZaposlenikId == zaposlenikId);

        var pendingInvite = user == null
            ? null
            : await _context.StaffInvitations.AsNoTracking()
                .Where(i => i.ZaposlenikId == zaposlenikId && i.AcceptedAt == null)
                .OrderByDescending(i => i.CreatedAt)
                .FirstOrDefaultAsync();

        var hasPassword = false;
        if (user != null)
        {
            hasPassword = await _userManager.HasPasswordAsync(user);
        }

        var email = (z.Email ?? user?.Email)?.Trim();
        var canInvite = !string.IsNullOrWhiteSpace(email) || !string.IsNullOrWhiteSpace(user?.Email);

        return new TherapistAccountStatusDto
        {
            ZaposlenikId = zaposlenikId,
            HasLinkedAccount = user != null,
            LinkedEmail = user?.Email ?? z.Email,
            LinkedUserName = user?.UserName,
            AccountActive = user?.Status ?? false,
            HasPassword = hasPassword,
            InvitePending = pendingInvite != null && pendingInvite.ExpiresAt > DateTime.UtcNow,
            InviteExpiresAt = pendingInvite?.ExpiresAt,
            CanInvite = canInvite,
            Message = user == null
                ? "No portal account linked."
                : hasPassword
                    ? "Therapist can sign in to the portal."
                    : "Invitation pending — therapist must set a password.",
        };
    }

    public async Task<TherapistInviteResponseDto> InviteAsync(
        int zaposlenikId,
        string? emailOverride,
        int? createdByKorisnikId,
        string inviteBaseUrl)
    {
        var z = await _context.Zaposlenici.FirstOrDefaultAsync(x => x.Id == zaposlenikId);
        if (z == null)
        {
            return Fail("Therapist profile not found.");
        }

        var email = (emailOverride ?? z.Email ?? "").Trim();
        if (string.IsNullOrWhiteSpace(email))
        {
            return Fail("Add an email on the therapist profile before sending an invite.");
        }

        if (!email.Contains('@'))
        {
            return Fail("A valid email address is required.");
        }

        var otherLinked = await _context.Users.AsNoTracking()
            .AnyAsync(k => k.ZaposlenikId == zaposlenikId && k.Email != email);
        if (otherLinked)
        {
            return Fail("Another user is already linked to this therapist profile.");
        }

        var existingByEmail = await _userManager.FindByEmailAsync(email);
        if (existingByEmail != null)
        {
            if (existingByEmail.ZaposlenikId.HasValue &&
                existingByEmail.ZaposlenikId.Value != zaposlenikId)
            {
                return Fail("This email is already linked to another therapist or account.");
            }

            if (await _userManager.IsInRoleAsync(existingByEmail, "Admin"))
            {
                return Fail("This email belongs to an administrator account.");
            }

            if (await _userManager.IsInRoleAsync(existingByEmail, "Klijent") &&
                !await _userManager.IsInRoleAsync(existingByEmail, "Zaposlenik"))
            {
                return Fail(
                    "This email is registered as a client. Use a different work email for the therapist portal.");
            }

            if (await _userManager.HasPasswordAsync(existingByEmail) &&
                existingByEmail.ZaposlenikId == zaposlenikId)
            {
                return Fail("This therapist already has an active portal account.");
            }
        }

        var user = existingByEmail ?? await CreateTherapistUserAsync(z, email);
        if (user == null)
        {
            return Fail("Could not create user account.");
        }

        user.ZaposlenikId = zaposlenikId;
        user.Email = email;
        user.NormalizedEmail = email.ToUpperInvariant();
        user.Ime = z.Ime.Trim();
        user.Prezime = z.Prezime.Trim();
        user.Status = true;
        if (string.IsNullOrWhiteSpace(z.Email))
        {
            z.Email = email;
        }

        var update = await _userManager.UpdateAsync(user);
        if (!update.Succeeded)
        {
            return Fail(string.Join("; ", update.Errors.Select(e => e.Description)));
        }

        if (!await _userManager.IsInRoleAsync(user, "Zaposlenik"))
        {
            await _userManager.AddToRoleAsync(user, "Zaposlenik");
        }

        await RevokePendingInvitesAsync(zaposlenikId);

        var rawToken = GenerateRawToken();
        var hash = HashToken(rawToken);
        var expires = DateTime.UtcNow.AddHours(InviteValidHours);

        _context.StaffInvitations.Add(new StaffInvitation
        {
            ZaposlenikId = zaposlenikId,
            KorisnikId = user.Id,
            Email = email,
            TokenHash = hash,
            ExpiresAt = expires,
            CreatedByKorisnikId = createdByKorisnikId,
        });

        await _context.SaveChangesAsync();

        var baseUrl = inviteBaseUrl.TrimEnd('/');
        var inviteUrl = $"{baseUrl}?token={Uri.EscapeDataString(rawToken)}";

        return new TherapistInviteResponseDto
        {
            Success = true,
            Message = "Invitation created. Share the activation link with the therapist (valid 72 hours).",
            InviteUrl = inviteUrl,
            ExpiresAt = expires,
            TherapistName = $"{z.Ime} {z.Prezime}".Trim(),
            RecipientEmail = email,
        };
    }

    public async Task<ValidateInviteTokenDto> ValidateInviteTokenAsync(string token)
    {
        var invite = await FindValidInviteAsync(token);
        if (invite == null)
        {
            return new ValidateInviteTokenDto
            {
                Valid = false,
                Message = "This invitation link is invalid or has expired.",
            };
        }

        var z = await _context.Zaposlenici.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == invite.ZaposlenikId);

        return new ValidateInviteTokenDto
        {
            Valid = true,
            TherapistName = z == null ? null : $"{z.Ime} {z.Prezime}".Trim(),
            Email = invite.Email,
            ExpiresAt = invite.ExpiresAt,
            Message = "Set your password to activate your therapist portal account.",
        };
    }

    public async Task<(bool Success, string Message, int? ActivatedUserId)> AcceptInviteAsync(
        string token,
        string password)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return (false, "Invitation token is required.", null);
        }

        if (password.Length < MinPasswordLength)
        {
            return (false, $"Password must be at least {MinPasswordLength} characters.", null);
        }

        var invite = await FindValidInviteAsync(token, tracked: true);
        if (invite == null)
        {
            return (false, "This invitation link is invalid or has expired.", null);
        }

        var user = await _userManager.FindByIdAsync(invite.KorisnikId.ToString());
        if (user == null)
        {
            return (false, "User account not found.", null);
        }

        IdentityResult result;
        if (await _userManager.HasPasswordAsync(user))
        {
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            result = await _userManager.ResetPasswordAsync(user, resetToken, password);
        }
        else
        {
            result = await _userManager.AddPasswordAsync(user, password);
        }

        if (!result.Succeeded)
        {
            return (false, string.Join("; ", result.Errors.Select(e => e.Description)), null);
        }

        user.Status = true;
        user.EmailConfirmed = true;
        user.ZaposlenikId = invite.ZaposlenikId;
        user.LockoutEnabled = true;
        await _userManager.UpdateAsync(user);

        invite.AcceptedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return (true, "Account activated. Signing you in…", user.Id);
    }

    private async Task<Korisnik?> CreateTherapistUserAsync(Zaposlenik z, string email)
    {
        var userName = await BuildUniqueUserNameAsync(email, z.Id);
        var user = new Korisnik
        {
            UserName = userName,
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = userName.ToUpperInvariant(),
            Ime = z.Ime.Trim(),
            Prezime = z.Prezime.Trim(),
            GradId = 1,
            Status = true,
            LockoutEnabled = true,
            DatumRegistracije = DateTime.UtcNow,
            ZaposlenikId = z.Id,
        };

        var create = await _userManager.CreateAsync(user);
        return create.Succeeded ? user : null;
    }

    private async Task<string> BuildUniqueUserNameAsync(string email, int zaposlenikId)
    {
        var local = email.Split('@')[0].Trim().ToLowerInvariant();
        local = Regex.Replace(local, @"[^a-z0-9._-]", "");
        if (string.IsNullOrWhiteSpace(local))
        {
            local = $"therapist{zaposlenikId}";
        }

        var candidate = local;
        var suffix = 0;
        while (await _userManager.FindByNameAsync(candidate) != null)
        {
            suffix++;
            candidate = $"{local}{suffix}";
        }

        return candidate;
    }

    private async Task RevokePendingInvitesAsync(int zaposlenikId)
    {
        var pending = await _context.StaffInvitations
            .Where(i => i.ZaposlenikId == zaposlenikId && i.AcceptedAt == null)
            .ToListAsync();

        foreach (var inv in pending)
        {
            inv.ExpiresAt = DateTime.UtcNow.AddSeconds(-1);
        }
    }

    private async Task<StaffInvitation?> FindValidInviteAsync(
        string rawToken,
        bool tracked = false)
    {
        if (string.IsNullOrWhiteSpace(rawToken)) return null;

        var hash = HashToken(rawToken.Trim());
        var now = DateTime.UtcNow;

        IQueryable<StaffInvitation> q = _context.StaffInvitations;
        if (!tracked)
        {
            q = q.AsNoTracking();
        }

        return await q
            .Include(i => i.Zaposlenik)
            .FirstOrDefaultAsync(i =>
                i.TokenHash == hash &&
                i.AcceptedAt == null &&
                i.ExpiresAt > now);
    }

    private static string GenerateRawToken()
    {
        var bytes = new byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static string HashToken(string raw)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw.Trim()));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static TherapistInviteResponseDto Fail(string message) =>
        new() { Success = false, Message = message };
}
