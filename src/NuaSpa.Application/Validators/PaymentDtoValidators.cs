using FluentValidation;
using NuaSpa.Application.DTOs;

namespace NuaSpa.Application.Validators;

public sealed class CreatePaymentIntentRequestDtoValidator : AbstractValidator<CreatePaymentIntentRequestDto>
{
    public CreatePaymentIntentRequestDtoValidator()
    {
        RuleFor(x => x.RezervacijaId)
            .GreaterThan(0)
            .WithMessage("Reservation id is required.");
    }
}

public sealed class ConfirmPaymentRequestDtoValidator : AbstractValidator<ConfirmPaymentRequestDto>
{
    public ConfirmPaymentRequestDtoValidator()
    {
        RuleFor(x => x.PaymentIntentId)
            .NotEmpty()
            .WithMessage("Payment intent id is required.");
    }
}

public sealed class RefundPaymentRequestDtoValidator : AbstractValidator<RefundPaymentRequestDto>
{
    public RefundPaymentRequestDtoValidator()
    {
        RuleFor(x => x.RezervacijaId)
            .GreaterThan(0)
            .WithMessage("Reservation id is required.");
    }
}

public sealed class RecordCashPaymentRequestDtoValidator : AbstractValidator<RecordCashPaymentRequestDto>
{
    public RecordCashPaymentRequestDtoValidator()
    {
        RuleFor(x => x.RezervacijaId)
            .GreaterThan(0)
            .WithMessage("Reservation id is required.");
        RuleFor(x => x.Iznos)
            .GreaterThan(0)
            .When(x => x.Iznos.HasValue)
            .WithMessage("Amount must be greater than zero.");
    }
}
