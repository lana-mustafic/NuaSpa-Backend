using FluentValidation;
using NuaSpa.Application.DTOs;

namespace NuaSpa.Application.Validators;

public sealed class RecenzijaCreateDtoValidator : AbstractValidator<RecenzijaCreateDTO>
{
    public RecenzijaCreateDtoValidator()
    {
        RuleFor(x => x.RezervacijaId)
            .GreaterThan(0).WithMessage("RezervacijaId je obavezan.");

        RuleFor(x => x.UslugaId)
            .GreaterThan(0).WithMessage("UslugaId je obavezan.");

        RuleFor(x => x.ZaposlenikId)
            .GreaterThan(0).WithMessage("ZaposlenikId je obavezan.");

        RuleFor(x => x.Ocjena)
            .InclusiveBetween(1, 5).WithMessage("Ocjena mora biti između 1 i 5.");

        RuleFor(x => x.Komentar)
            .NotEmpty().WithMessage("Komentar je obavezan.")
            .MaximumLength(1000)
            .WithMessage("Komentar može imati najviše 1000 znakova.");
    }
}
