using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuaSpa.Api.Extensions;
using NuaSpa.Application.Common;
using NuaSpa.Domain;
using NuaSpa.Domain.Entities;
using Stripe;

namespace NuaSpa.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Klijent,Admin")]
    public class PlacanjeController : ControllerBase
    {
        private readonly NuaSpaContext _context;
        private readonly StripeSettings _stripe;

        public PlacanjeController(NuaSpaContext context, StripeSettings stripe)
        {
            _context = context;
            _stripe = stripe;
        }

        public class CreatePaymentIntentRequest
        {
            public int RezervacijaId { get; set; }
        }

        public class CreatePaymentIntentResponse
        {
            public string ClientSecret { get; set; } = string.Empty;
            public string PaymentIntentId { get; set; } = string.Empty;
            public string Currency { get; set; } = string.Empty;
            public long Amount { get; set; }
        }

        [HttpPost("create-intent")]
        public async Task<ActionResult<CreatePaymentIntentResponse>> CreateIntent([FromBody] CreatePaymentIntentRequest request)
        {
            if (string.IsNullOrWhiteSpace(_stripe.SecretKey))
                return BadRequest("Stripe SecretKey nije konfigurisan (postavite Stripe__SecretKey u .env).");

            var userId = User.GetNuaSpaUserId();

            var rezervacija = await _context.Rezervacije
                .Include(r => r.Usluga)
                .FirstOrDefaultAsync(r => r.Id == request.RezervacijaId);

            if (rezervacija == null) return NotFound("Rezervacija nije pronađena.");
            if (!User.IsInRole("Admin") && rezervacija.KorisnikId != userId) return Forbid();

            if (rezervacija.IsPlacena)
                return BadRequest("Rezervacija je već plaćena.");

            // BAM nema minor unit (1 BAM = 100 feninga -> ipak koristimo 2 decimale kao standard)
            // Stripe očekuje integer u najmanjoj valuti. Pretpostavka: 2 decimale.
            var amount = (long)decimal.Round(rezervacija.Usluga.Cijena * 100m, 0);

            var intentService = new PaymentIntentService();
            var intent = await intentService.CreateAsync(new PaymentIntentCreateOptions
            {
                Amount = amount,
                Currency = _stripe.Currency,
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true
                },
                Metadata = new Dictionary<string, string>
                {
                    ["rezervacijaId"] = rezervacija.Id.ToString(),
                    ["korisnikId"] = rezervacija.KorisnikId.ToString()
                }
            });

            // upiši u bazu (MVP)
            _context.Placanja.Add(new Placanje
            {
                Iznos = rezervacija.Usluga.Cijena,
                DatumPlacanja = DateTime.Now,
                MetodaPlacanja = "Stripe",
                TransakcijskiBroj = intent.Id,
                RezervacijaId = rezervacija.Id
            });
            await _context.SaveChangesAsync();

            return Ok(new CreatePaymentIntentResponse
            {
                ClientSecret = intent.ClientSecret,
                PaymentIntentId = intent.Id,
                Currency = _stripe.Currency,
                Amount = amount
            });
        }

        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> Webhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            if (string.IsNullOrWhiteSpace(_stripe.WebhookSecret))
                return BadRequest("Stripe WebhookSecret nije konfigurisan.");

            Event stripeEvent;
            try
            {
                stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    _stripe.WebhookSecret
                );
            }
            catch
            {
                return BadRequest();
            }

            if (stripeEvent.Type == "payment_intent.succeeded")
            {
                var intent = stripeEvent.Data.Object as PaymentIntent;
                if (intent != null)
                {
                    var placanje = await _context.Placanja
                        .Include(p => p.Rezervacija)
                        .FirstOrDefaultAsync(p => p.TransakcijskiBroj == intent.Id);

                    if (placanje?.Rezervacija != null)
                    {
                        placanje.Rezervacija.IsPlacena = true;
                        await _context.SaveChangesAsync();
                    }
                }
            }

            return Ok();
        }
    }
}

