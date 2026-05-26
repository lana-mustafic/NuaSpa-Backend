using FluentValidation;
using NuaSpa.Application.DTOs;

namespace NuaSpa.Application.Validators;

public sealed class RezervacijaCreateDtoValidator : AbstractValidator<RezervacijaCreateDTO>
{
    public RezervacijaCreateDtoValidator()
    {
        RuleFor(x => x.UslugaId)
            .GreaterThan(0).WithMessage("Odaberite uslugu.");

        RuleFor(x => x.ZaposlenikId)
            .GreaterThan(0).WithMessage("Odaberite terapeuta.");

        RuleFor(x => x.DatumRezervacije)
            .Must(d => d != default)
            .WithMessage("Datum i vrijeme termina su obavezni.");

        RuleFor(x => x.KorisnikId)
            .GreaterThan(0).When(x => x.KorisnikId.HasValue)
            .WithMessage("Odaberite klijenta.");
    }
}
