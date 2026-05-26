using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Stripe;
using NuaSpa.Application.Common;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.Exceptions;
using NuaSpa.Application.Interfaces;
using NuaSpa.Domain;
using NuaSpa.Domain.Entities;
using Stripe.Infrastructure;

namespace NuaSpa.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly NuaSpaContext _context;
    private readonly StripeSettings _stripe;

    public PaymentService(NuaSpaContext context, StripeSettings stripe)
    {
        _context = context;
        _stripe = stripe;
    }

    public async Task<CreatePaymentIntentResponseDto> CreatePaymentIntentAsync(
        int rezervacijaId,
        int userId,
        bool isAdminBooking,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_stripe.SecretKey))
        {
            throw new BusinessRuleException(
                "Stripe SecretKey nije konfigurisan. (postavi Stripe__SecretKey u .env)");
        }

        StripeConfiguration.ApiKey = _stripe.SecretKey;

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

        if (rezervacija.IsPlacena)
        {
            throw new BusinessRuleException("Rezervacija je već plaćena.");
        }

        // Stripe očekuje integer u najmanjoj valuti (pretpostavka: 2 decimale).
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
        }, cancellationToken: ct);

        // Upis u bazu (MVP).
        _context.Placanja.Add(new Placanje
        {
            Iznos = rezervacija.Usluga.Cijena,
            DatumPlacanja = DateTime.UtcNow,
            MetodaPlacanja = "Stripe",
            TransakcijskiBroj = intent.Id,
            RezervacijaId = rezervacija.Id
        });

        await _context.SaveChangesAsync(ct);

        return new CreatePaymentIntentResponseDto
        {
            ClientSecret = intent.ClientSecret,
            PaymentIntentId = intent.Id,
            Currency = _stripe.Currency,
            Amount = amount
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
            // Ne izlagati infrastrukturu/razlog klijentu.
            throw new BusinessRuleException("Neispravan Stripe webhook zahtjev.");
        }

        if (stripeEvent.Type == "payment_intent.succeeded")
        {
            var intent = stripeEvent.Data.Object as PaymentIntent;
            if (intent != null)
            {
                var placanje = await _context.Placanja
                    .Include(p => p.Rezervacija)
                    .FirstOrDefaultAsync(
                        p => p.TransakcijskiBroj == intent.Id,
                        ct);

                if (placanje?.Rezervacija != null)
                {
                    placanje.Rezervacija.IsPlacena = true;
                    await _context.SaveChangesAsync(ct);
                }
            }
        }
    }
}

