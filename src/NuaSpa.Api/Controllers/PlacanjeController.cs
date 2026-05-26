using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuaSpa.Api.Extensions;
using NuaSpa.Application.Common;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.Interfaces;
using System.IO;
using System.Threading;

namespace NuaSpa.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = RoleConstants.KlijentAdmin)]
    public class PlacanjeController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PlacanjeController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost("create-intent")]
        public async Task<ActionResult<CreatePaymentIntentResponseDto>> CreateIntent(
            [FromBody] CreatePaymentIntentRequestDto request,
            CancellationToken ct = default)
        {
            var userId = User.GetNuaSpaUserId();
            var isAdminBooking = User.IsInRole(RoleConstants.Admin);

            var dto = await _paymentService.CreatePaymentIntentAsync(
                request.RezervacijaId,
                userId,
                isAdminBooking,
                ct);

            return Ok(dto);
        }

        /// <summary>Stripe webhook — anoniman, ali zaštićen provjerom potpisa (ConstructEvent).</summary>
        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> Webhook(CancellationToken ct = default)
        {
            using var reader = new StreamReader(HttpContext.Request.Body);
            var json = await reader.ReadToEndAsync(ct);
            var signature = Request.Headers["Stripe-Signature"].ToString();

            await _paymentService.HandleStripeWebhookAsync(json, signature, ct);

            return Ok();
        }
    }
}

