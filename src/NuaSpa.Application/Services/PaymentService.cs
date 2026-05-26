using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Stripe;
using NuaSpa.Application.Common;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.Exceptions;
using NuaSpa.Application.Interfaces;
using NuaSpa.Domain;
using NuaSpa.Application.Services.Booking;
using NuaSpa.Domain.Enums;
using NuaSpa.Domain.Entities;

namespace NuaSpa.Application.Services;

public class PaymentService : IPaymentService
{
    private static readonly HashSet<string> ReusableIntentStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "requires_payment_method",
        "requires_confirmation",
        "requires_action",
        "processing",
    };

    private readonly NuaSpaContext _context;
    private readonly StripeSettings _stripe;
    private readonly ISistemskaNotifikacijaService _notifikacije;

    public PaymentService(
        NuaSpaContext context,
        StripeSettings stripe,
        ISistemskaNotifikacijaService notifikacije)
    {
        _context = context;
        _stripe = stripe;
        _notifikacije = notifikacije;
    }

    public async Task<CreatePaymentIntentResponseDto> CreatePaymentIntentAsync(
        int rezervacijaId,
        int userId,
        bool isAdminBooking,
        CancellationToken ct)
    {
        EnsureStripeSecretConfigured();
        StripeConfiguration.ApiKey = _stripe.SecretKey;

        var rezervacija = await LoadReservationForPaymentAsync(rezervacijaId, userId, isAdminBooking, ct);
        ValidateReservationPayable(rezervacija);

        var chargeAmount = RezervacijaPricing.ResolveChargeAmount(
            rezervacija.Usluga.Cijena,
            rezervacija.SnimakCijena);
        var amountMinor = RezervacijaPricing.ToStripeMinorUnits(chargeAmount);

        var existingPayments = await _context.Placanja
            .Where(p => p.RezervacijaId == rezervacijaId && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);

        foreach (var existing in existingPayments)
        {
            if (existing.Status == PlacanjeStatus.Completed)
            {
                throw new BusinessRuleException("Rezervacija je već plaćena.");
            }

            if (existing.Status == PlacanjeStatus.Refunded)
            {
                continue;
            }

            if (existing.Status != PlacanjeStatus.Pending)
            {
                continue;
            }

            var intentService = new PaymentIntentService();
            var existingIntent = await intentService.GetAsync(
                existing.TransakcijskiBroj,
                cancellationToken: ct);

            if (string.Equals(existingIntent.Status, "succeeded", StringComparison.OrdinalIgnoreCase))
            {
                await FinalizePaymentAsync(existing, existingIntent, ct);
                throw new BusinessRuleException("Rezervacija je već plaćena.");
            }

            if (ReusableIntentStatuses.Contains(existingIntent.Status))
            {
                return new CreatePaymentIntentResponseDto
                {
                    ClientSecret = existingIntent.ClientSecret,
                    PaymentIntentId = existingIntent.Id,
                    Currency = _stripe.Currency,
                    Amount = existingIntent.Amount,
                };
            }

            existing.Status = PlacanjeStatus.Failed;
        }

        var intentServiceCreate = new PaymentIntentService();
        var intent = await intentServiceCreate.CreateAsync(new PaymentIntentCreateOptions
        {
            Amount = amountMinor,
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
        }, cancellationToken: ct);

        _context.Placanja.Add(new Placanje
        {
            Iznos = chargeAmount,
            DatumPlacanja = DateTime.UtcNow,
            MetodaPlacanja = "Stripe",
            TransakcijskiBroj = intent.Id,
            RezervacijaId = rezervacija.Id,
            Status = PlacanjeStatus.Pending,
        });

        await _context.SaveChangesAsync(ct);

        return new CreatePaymentIntentResponseDto
        {
            ClientSecret = intent.ClientSecret,
            PaymentIntentId = intent.Id,
            Currency = _stripe.Currency,
            Amount = amountMinor
        };
    }

    public async Task<ConfirmPaymentResponseDto> ConfirmPaymentAsync(
        string paymentIntentId,
        int userId,
        bool isAdminBooking,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(paymentIntentId))
        {
            throw new BusinessRuleException("PaymentIntentId je obavezan.");
        }

        EnsureStripeSecretConfigured();
        StripeConfiguration.ApiKey = _stripe.SecretKey;

        var placanje = await _context.Placanja
            .Include(p => p.Rezervacija!)
                .ThenInclude(r => r.Usluga)
            .FirstOrDefaultAsync(p => p.TransakcijskiBroj == paymentIntentId, ct);

        if (placanje?.Rezervacija == null)
        {
            throw new NotFoundException("Plaćanje nije pronađeno.");
        }

        if (!isAdminBooking && placanje.Rezervacija.KorisnikId != userId)
        {
            throw new ForbiddenException("Nemate dozvolu za potvrdu ovog plaćanja.");
        }

        if (placanje.Status == PlacanjeStatus.Completed && placanje.Rezervacija.IsPlacena)
        {
            return BuildConfirmResponse(placanje, alreadyCompleted: true);
        }

        if (placanje.Status == PlacanjeStatus.Refunded)
        {
            throw new BusinessRuleException("Plaćanje je refundirano.");
        }

        var intentService = new PaymentIntentService();
        var intent = await intentService.GetAsync(paymentIntentId, cancellationToken: ct);

        if (!string.Equals(intent.Status, "succeeded", StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessRuleException("Plaćanje nije uspješno završeno na Stripe-u.");
        }

        return await FinalizePaymentAsync(placanje, intent, ct);
    }

    public async Task<RefundPaymentResponseDto> RefundPaymentAsync(
        int rezervacijaId,
        int userId,
        CancellationToken ct)
    {
        var placanje = await FindStripePlacanjeForRefundAsync(rezervacijaId, requireCompleted: true, ct);
        if (placanje == null)
        {
            throw new NotFoundException("Nema završenog Stripe plaćanja za refund.");
        }

        return await ExecuteStripeRefundAsync(placanje, ct);
    }

    public async Task<RefundPaymentResponseDto?> RefundIfPaidAsync(
        int rezervacijaId,
        CancellationToken ct)
    {
        var placanje = await FindStripePlacanjeForRefundAsync(rezervacijaId, requireCompleted: false, ct);
        if (placanje == null)
        {
            return null;
        }

        if (placanje.Status == PlacanjeStatus.Refunded)
        {
            return BuildRefundResponse(placanje, alreadyRefunded: true);
        }

        return await ExecuteStripeRefundAsync(placanje, ct);
    }

    private async Task<Placanje?> FindStripePlacanjeForRefundAsync(
        int rezervacijaId,
        bool requireCompleted,
        CancellationToken ct)
    {
        var query = _context.Placanja
            .Include(p => p.Rezervacija)
            .Where(p => p.RezervacijaId == rezervacijaId && !p.IsDeleted)
            .Where(p =>
                (p.MetodaPlacanja ?? "").ToLower().Contains("stripe")
                || (p.TransakcijskiBroj ?? "").StartsWith("pi_"));

        if (requireCompleted)
        {
            query = query.Where(p => p.Status == PlacanjeStatus.Completed);
        }
        else
        {
            query = query.Where(p =>
                p.Status == PlacanjeStatus.Completed || p.Status == PlacanjeStatus.Refunded);
        }

        return await query
            .OrderByDescending(p => p.DatumZavrsetka ?? p.DatumPlacanja)
            .FirstOrDefaultAsync(ct);
    }

    private async Task<RefundPaymentResponseDto> ExecuteStripeRefundAsync(
        Placanje placanje,
        CancellationToken ct)
    {
        if (placanje.Rezervacija == null)
        {
            await _context.Entry(placanje).Reference(p => p.Rezervacija).LoadAsync(ct);
        }

        if (placanje.Rezervacija == null)
        {
            throw new BusinessRuleException("Rezervacija nije povezana s plaćanjem.");
        }

        if (placanje.Status == PlacanjeStatus.Refunded)
        {
            return BuildRefundResponse(placanje, alreadyRefunded: true);
        }

        EnsureStripeSecretConfigured();
        StripeConfiguration.ApiKey = _stripe.SecretKey;

        var intentService = new PaymentIntentService();
        var intent = await intentService.GetAsync(placanje.TransakcijskiBroj, cancellationToken: ct);

        var refundMinor = intent.AmountReceived > 0 ? intent.AmountReceived : intent.Amount;
        if (refundMinor <= 0)
        {
            throw new BusinessRuleException("Nema naplaćenog iznosa za refund.");
        }

        var refundService = new RefundService();
        var refund = await refundService.CreateAsync(new RefundCreateOptions
        {
            PaymentIntent = intent.Id,
            Amount = refundMinor,
        }, cancellationToken: ct);

        var refundedAmount = FromStripeMinorUnits(refundMinor);
        placanje.Status = PlacanjeStatus.Refunded;
        placanje.StripeRefundId = refund.Id;
        placanje.Rezervacija.IsPlacena = false;

        await _context.SaveChangesAsync(ct);

        if (placanje.Rezervacija != null)
        {
            try
            {
                await _notifikacije.NotifyPlacanjeRefundiranoAsync(
                    placanje.Rezervacija,
                    refundedAmount,
                    ct);
            }
            catch
            {
                // Notifikacije ne smiju prekinuti refund.
            }
        }

        return new RefundPaymentResponseDto
        {
            RefundId = refund.Id,
            RefundedAmount = refundedAmount,
            IsRefunded = true,
        };
    }

    private static RefundPaymentResponseDto BuildRefundResponse(Placanje placanje, bool alreadyRefunded)
    {
        return new RefundPaymentResponseDto
        {
            RefundId = placanje.StripeRefundId ?? string.Empty,
            RefundedAmount = placanje.NaplaceniIznos ?? placanje.Iznos,
            IsRefunded = alreadyRefunded,
        };
    }

    public async Task HandleStripeWebhookAsync(
        string requestBodyJson,
        string stripeSignatureHeader,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_stripe.WebhookSecret))
        {
            throw new BusinessRuleException("Stripe WebhookSecret nije konfigurisan.");
        }

        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(
                requestBodyJson,
                stripeSignatureHeader,
                _stripe.WebhookSecret);
        }
        catch (Exception)
        {
            throw new BusinessRuleException("Neispravan Stripe webhook zahtjev.");
        }

        var alreadyProcessed = await _context.StripeWebhookEvents
            .AnyAsync(e => e.StripeEventId == stripeEvent.Id, ct);
        if (alreadyProcessed)
        {
            return;
        }

        EnsureStripeSecretConfigured();
        StripeConfiguration.ApiKey = _stripe.SecretKey;

        if (stripeEvent.Type == "payment_intent.succeeded")
        {
            var intent = stripeEvent.Data.Object as PaymentIntent;
            if (intent != null)
            {
                var placanje = await _context.Placanja
                    .Include(p => p.Rezervacija)
                    .FirstOrDefaultAsync(p => p.TransakcijskiBroj == intent.Id, ct);

                if (placanje?.Rezervacija != null)
                {
                    await FinalizePaymentAsync(placanje, intent, ct);
                }
            }
        }
        else if (stripeEvent.Type == "charge.refunded")
        {
            var charge = stripeEvent.Data.Object as Charge;
            if (charge?.PaymentIntentId != null)
            {
                var placanje = await _context.Placanja
                    .Include(p => p.Rezervacija)
                    .FirstOrDefaultAsync(p => p.TransakcijskiBroj == charge.PaymentIntentId, ct);

                if (placanje?.Rezervacija != null
                    && placanje.Status != PlacanjeStatus.Refunded)
                {
                    placanje.Status = PlacanjeStatus.Refunded;
                    placanje.Rezervacija.IsPlacena = false;
                    if (charge.AmountRefunded > 0)
                    {
                        placanje.NaplaceniIznos = FromStripeMinorUnits(charge.AmountRefunded);
                    }

                    await _context.SaveChangesAsync(ct);
                }
            }
        }

        _context.StripeWebhookEvents.Add(new StripeWebhookEvent
        {
            StripeEventId = stripeEvent.Id,
            EventType = stripeEvent.Type,
            ProcessedAtUtc = DateTime.UtcNow,
        });

        await _context.SaveChangesAsync(ct);
    }

    private async Task<Rezervacija> LoadReservationForPaymentAsync(
        int rezervacijaId,
        int userId,
        bool isAdminBooking,
        CancellationToken ct)
    {
        var rezervacija = await _context.Rezervacije
            .Include(r => r.Usluga)
            .FirstOrDefaultAsync(r => r.Id == rezervacijaId, ct);

        if (rezervacija == null)
        {
            throw new NotFoundException("Rezervacija nije pronađena.");
        }

        if (!isAdminBooking && rezervacija.KorisnikId != userId)
        {
            throw new ForbiddenException("Nemate dozvolu za kreiranje plaćanja za ovu rezervaciju.");
        }

        return rezervacija;
    }

    private static void ValidateReservationPayable(Rezervacija rezervacija)
    {
        if (rezervacija.IsPlacena)
        {
            throw new BusinessRuleException("Rezervacija je već plaćena.");
        }

        if (rezervacija.Status == RezervacijaStatus.Cancelled)
        {
            throw new BusinessRuleException("Otkazana rezervacija se ne može platiti.");
        }

        if (rezervacija.Status != RezervacijaStatus.Confirmed)
        {
            throw new BusinessRuleException("Plaćanje je moguće samo za potvrđenu rezervaciju.");
        }
    }

    private async Task<ConfirmPaymentResponseDto> FinalizePaymentAsync(
        Placanje placanje,
        PaymentIntent intent,
        CancellationToken ct)
    {
        if (placanje.Rezervacija == null)
        {
            await _context.Entry(placanje).Reference(p => p.Rezervacija).LoadAsync(ct);
        }

        var rezervacija = placanje.Rezervacija
            ?? throw new BusinessRuleException("Rezervacija nije povezana s plaćanjem.");

        if (placanje.Status == PlacanjeStatus.Completed && rezervacija.IsPlacena)
        {
            return BuildConfirmResponse(placanje, alreadyCompleted: true);
        }

        var wasAlreadyCompleted = placanje.Status == PlacanjeStatus.Completed;
        var chargedMinor = intent.AmountReceived > 0 ? intent.AmountReceived : intent.Amount;
        var chargedAmount = FromStripeMinorUnits(chargedMinor);

        placanje.NaplaceniIznos = chargedAmount;
        placanje.Iznos = chargedAmount;
        placanje.Status = PlacanjeStatus.Completed;
        placanje.DatumZavrsetka = DateTime.UtcNow;
        rezervacija.IsPlacena = true;

        await _context.SaveChangesAsync(ct);

        if (!wasAlreadyCompleted)
        {
            try
            {
                await _notifikacije.NotifyPlacanjeUspjesnoAsync(rezervacija, chargedAmount, ct);
            }
            catch
            {
                // Notifikacije ne smiju prekinuti plaćanje.
            }
        }

        return BuildConfirmResponse(placanje, alreadyCompleted: false);
    }

    private static ConfirmPaymentResponseDto BuildConfirmResponse(Placanje placanje, bool alreadyCompleted)
    {
        var amount = placanje.NaplaceniIznos ?? placanje.Iznos;
        return new ConfirmPaymentResponseDto
        {
            IsPlacena = true,
            IsPaid = true,
            AlreadyCompleted = alreadyCompleted,
            ChargedAmount = amount,
        };
    }

    private static decimal FromStripeMinorUnits(long minorUnits)
    {
        return decimal.Round(minorUnits / 100m, 2, MidpointRounding.AwayFromZero);
    }

    private void EnsureStripeSecretConfigured()
    {
        if (string.IsNullOrWhiteSpace(_stripe.SecretKey))
        {
            throw new BusinessRuleException(
                "Stripe SecretKey nije konfigurisan. (postavi Stripe__SecretKey u .env)");
        }
    }
}
