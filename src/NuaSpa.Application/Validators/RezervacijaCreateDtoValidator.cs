using FluentValidation;
using NuaSpa.Application.DTOs;

namespace NuaSpa.Application.Validators;

public sealed class RezervacijaCreateDtoValidator : AbstractValidator<RezervacijaCreateDTO>
{
    public RezervacijaCreateDtoValidator()
    {
        RuleFor(x => x.UslugaId)
            .GreaterThan(0).WithMessage("UslugaId je obavezan.");

        RuleFor(x => x.ZaposlenikId)
            .GreaterThan(0).WithMessage("ZaposlenikId je obavezan.");

        RuleFor(x => x.DatumRezervacije)
            .Must(d => d != default)
            .WithMessage("Datum rezervacije je obavezan.");

        RuleFor(x => x.KorisnikId)
            .GreaterThan(0).When(x => x.KorisnikId.HasValue)
            .WithMessage("KorisnikId mora biti pozitivan broj.");
    }
}
