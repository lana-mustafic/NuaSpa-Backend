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

        /// <summary>
        /// Server-side verifikacija uspješnog plaćanja (Stripe API). Idempotentan — ne ponavlja efekte ako je već completed.
        /// </summary>
        [HttpPost("confirm")]
        public async Task<ActionResult<ConfirmPaymentResponseDto>> Confirm(
            [FromBody] ConfirmPaymentRequestDto request,
            CancellationToken ct = default)
        {
            var userId = User.GetNuaSpaUserId();
            var isAdminBooking = User.IsInRole(RoleConstants.Admin);

            var dto = await _paymentService.ConfirmPaymentAsync(
                request.PaymentIntentId,
                userId,
                isAdminBooking,
                ct);

            return Ok(dto);
        }

        /// <summary>Refund na osnovu stvarno naplaćenog iznosa (Stripe PaymentIntent).</summary>
        [HttpPost("refund")]
        [Authorize(Roles = RoleConstants.Admin)]
        public async Task<ActionResult<RefundPaymentResponseDto>> Refund(
            [FromBody] RefundPaymentRequestDto request,
            CancellationToken ct = default)
        {
            var userId = User.GetNuaSpaUserId();

            var dto = await _paymentService.RefundPaymentAsync(
                request.RezervacijaId,
                userId,
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
