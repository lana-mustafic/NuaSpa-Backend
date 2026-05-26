using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuaSpa.Api.Extensions;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.Interfaces;
using NuaSpa.Application.Interfaces.Messaging;
using NuaSpa.Application.Messaging.Messages;
using NuaSpa.Application.Common;

namespace NuaSpa.Api.Controllers;

[ApiController]
[Route("api/admin/therapists/{zaposlenikId:int}/account")]
[Authorize(Roles = RoleConstants.Admin)]
public class AdminTherapistAccountController : ControllerBase
{
    private readonly ITherapistAccountService _accountService;
    private readonly INotificationPublisher _notificationPublisher;
    private readonly string _therapistInviteBaseUrl;
    private readonly ILogger<AdminTherapistAccountController> _logger;

    public AdminTherapistAccountController(
        ITherapistAccountService accountService,
        INotificationPublisher notificationPublisher,
        IConfiguration configuration,
        ILogger<AdminTherapistAccountController> logger)
    {
        _accountService = accountService;
        _notificationPublisher = notificationPublisher;
        var baseUrl = configuration["TherapistInvite:BaseUrl"]?.Trim();
        _therapistInviteBaseUrl = string.IsNullOrWhiteSpace(baseUrl)
            ? "nuaspa://accept-invite"
            : baseUrl;
        _logger = logger;
    }

    [HttpGet("status")]
    public async Task<ActionResult<TherapistAccountStatusDto>> GetStatus(int zaposlenikId)
    {
        var status = await _accountService.GetAccountStatusAsync(zaposlenikId);
        if (status == null) return NotFound();
        return Ok(status);
    }

    [HttpPost("invite")]
    public async Task<ActionResult<TherapistInviteResponseDto>> Invite(
        int zaposlenikId,
        [FromBody] TherapistInviteRequestDto? body)
    {
        int? adminId = null;
        if (User.TryGetNuaSpaUserId(out var id))
        {
            adminId = id;
        }

        var result = await _accountService.InviteAsync(
            zaposlenikId,
            body?.Email,
            adminId,
            _therapistInviteBaseUrl);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        if (!string.IsNullOrWhiteSpace(result.RecipientEmail) &&
            !string.IsNullOrWhiteSpace(result.InviteUrl) &&
            result.ExpiresAt.HasValue)
        {
            try
            {
                await _notificationPublisher.PublishTherapistInviteAsync(new TherapistInviteEmailMessage
                {
                    ToEmail = result.RecipientEmail,
                    TherapistName = result.TherapistName ?? result.RecipientEmail,
                    InviteUrl = result.InviteUrl,
                    ExpiresAtUtc = result.ExpiresAt.Value,
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "RabbitMQ pozivnica terapeutu nije poslana.");
            }
        }

        return Ok(result);
    }
}
